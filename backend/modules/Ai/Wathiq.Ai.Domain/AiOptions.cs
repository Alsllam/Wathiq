using System;

namespace Wathiq.Ai;

/// <summary>One logical client: which provider serves which model at which endpoint.</summary>
public class AiClientOptions
{
    public string Provider { get; set; } = AiConsts.OllamaProvider;
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "qwen2.5:7b";
}

/// <summary>Bound from configuration section "Ai" (host appsettings / env vars).</summary>
public class AiOptions
{
    public AiClientOptions Extraction { get; set; } = new();
    public AiClientOptions Guides { get; set; } = new();
    /// <summary>D2: bge-m3 - multilingual (ar/en), 1024 dims. Local-only, like Extraction.</summary>
    public AiClientOptions Embeddings { get; set; } = new() { Model = "bge-m3" };

    /// <summary>FR-AI-004: model calls per user per UTC day, across all purposes.</summary>
    public int DailyCallCapPerUser { get; set; } = 50;

    /// <summary>
    /// FR-AI-002 / C1 as a startup guard: personal documents are only ever extracted by the
    /// self-hosted provider. A config typo pointing extraction at a cloud tier must kill the
    /// boot, not silently leak documents.
    /// </summary>
    public void Validate()
    {
        if (!string.Equals(Extraction.Provider, AiConsts.OllamaProvider, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Ai:Extraction.Provider is '{Extraction.Provider}' but document extraction must use " +
                $"the self-hosted provider '{AiConsts.OllamaProvider}' (FR-AI-002). Refusing to start.");
        }

        // Guide TEXT is public, but embedding requests would still ship it (and later, user
        // questions) to a third party - so embeddings get the same wall as extraction.
        if (!string.Equals(Embeddings.Provider, AiConsts.OllamaProvider, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Ai:Embeddings.Provider is '{Embeddings.Provider}' but embeddings must use " +
                $"the self-hosted provider '{AiConsts.OllamaProvider}' (C1). Refusing to start.");
        }
    }
}
