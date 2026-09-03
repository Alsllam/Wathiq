using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Wathiq.Guides;

/// <summary>
/// Deterministic stand-in for bge-m3: 8-dim vectors derived from the text's hash, so tests can
/// assert exact stored bytes without a model. Records inputs like every fake in this suite.
/// </summary>
public class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public List<string> Inputs { get; } = [];

    public static float[] VectorFor(string text)
    {
        // Stable across runs (no string.GetHashCode randomization): sum of chars seeds the ramp.
        var seed = 0;
        foreach (var c in text) seed = unchecked(seed * 31 + c);
        return Enumerable.Range(0, 8).Select(i => (float)Math.Sin(seed + i)).ToArray();
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var list = values.ToList();
        Inputs.AddRange(list);
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(
            list.Select(v => new Embedding<float>(VectorFor(v)) { ModelId = "fake-embed" })));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
