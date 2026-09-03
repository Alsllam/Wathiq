using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wathiq.Guides.Retrieval;

/// <summary>One retrieved chunk with its score - the id pair is what 5.5's citations point at.</summary>
public record GuideChunkMatch(Guid ChunkId, Guid GuideVersionId, int ChunkNo, string Text, double Score);

/// <summary>
/// FR-GDE-004's retrieval half: embed the question, cosine against the served corpus, return
/// the top matches above the floor. An EMPTY list is a meaningful answer ("nothing relevant") -
/// the chat layer turns it into an honest refusal, never into a guess.
/// </summary>
public interface IGuideRetriever
{
    Task<IReadOnlyList<GuideChunkMatch>> RetrieveAsync(
        string question, int? topK = null, double? similarityFloor = null, CancellationToken cancellationToken = default);
}
