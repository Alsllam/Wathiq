using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wathiq.Ai;

namespace Wathiq.HealthChecks;

/// <summary>
/// Probes the local model server the way the database check probes SQL: a stateful dependency
/// whose absence must be VISIBLE, not fatal. Reports Degraded (never Unhealthy) because every
/// non-AI feature - documents, reminders - keeps working when Ollama is down.
/// </summary>
public class AiHealthCheck : IHealthCheck
{
    // One client for the process: the probe is tiny and periodic; sockets must not pile up.
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(3) };

    private readonly AiOptions _options;

    public AiHealthCheck(AiOptions options)
    {
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var endpoint = _options.Extraction.Endpoint.TrimEnd('/');
        try
        {
            // /api/version: the cheapest thing Ollama serves; proves the server, not any model.
            var response = await Http.GetAsync($"{endpoint}/api/version", cancellationToken);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            return HealthCheckResult.Healthy($"Ollama at {endpoint} answered: {body.Trim()}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                $"Ollama at {endpoint} is unreachable - extraction and guides chat are unavailable " +
                $"until it is started (see backend/README.md, AI section).", ex);
        }
    }
}
