using System;

namespace Wathiq.Guides.Guides;

/// <summary>
/// The whole "vector database" kernel: ten lines of arithmetic. Cosine = dot(a,b)/(|a||b|),
/// direction-only similarity - two texts about fees score high even if one is much longer.
/// Accumulates in double: 1024 float32 products lose real precision summed in float.
/// </summary>
public static class VectorMath
{
    public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
        {
            // Mixed dimensionality means mixed models - a bug upstream, never a low score.
            throw new ArgumentException($"Vector lengths differ: {a.Length} vs {b.Length}.");
        }

        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += (double)a[i] * b[i];
            magA += (double)a[i] * a[i];
            magB += (double)b[i] * b[i];
        }

        if (magA == 0 || magB == 0)
        {
            return 0;   // a zero vector is similar to nothing, not NaN-similar to everything
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
