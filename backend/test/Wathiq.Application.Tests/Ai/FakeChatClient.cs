using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Wathiq.Ai;

/// <summary>The model behind a seam: counts calls, returns a fixed answer with token usage.</summary>
public class FakeChatClient : IChatClient
{
    public int CallCount { get; private set; }
    /// <summary>Scriptable since 3.6: extractor tests feed it model JSON. Default keeps 3.3's tests as-is.</summary>
    public string NextResponseText { get; set; } = "ok";

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, NextResponseText))
        {
            Usage = new UsageDetails { InputTokenCount = 120, OutputTokenCount = 30 }
        });
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        CallCount++;
        yield return new ChatResponseUpdate(ChatRole.Assistant, "ok");
        await Task.CompletedTask;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
