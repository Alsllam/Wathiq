using System;

namespace Wathiq.Guides;

/// <summary>Bound from configuration section "Guides:Retrieval"; defaults are the product decision.</summary>
public class GuideRetrievalOptions
{
    /// <summary>Chunks handed to the chat prompt (5.5) - few and relevant beats many and noisy.</summary>
    public int TopK { get; set; } = 4;

    /// <summary>
    /// Below this cosine score a chunk is "not actually about the question" - the refusal
    /// signal (5.5): better an honest "no answer" than a confident answer from noise.
    /// Tuned against the 5.6 eval set; 0.5 is the pre-eval starting point.
    /// </summary>
    public double SimilarityFloor { get; set; } = 0.5;

    /// <summary>How long the hydrated corpus may be served before re-reading SQL.</summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(5);
}
