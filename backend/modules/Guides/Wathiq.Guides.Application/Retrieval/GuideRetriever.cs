using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.Retrieval;

public class GuideRetriever : IGuideRetriever, ITransientDependency
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IRepository<Guide, Guid> _guides;
    private readonly IRepository<GuideVersion, Guid> _versions;
    private readonly IRepository<GuideChunk, Guid> _chunks;
    private readonly GuideChunkCache _cache;
    private readonly GuideRetrievalOptions _options;

    public GuideRetriever(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IRepository<Guide, Guid> guides,
        IRepository<GuideVersion, Guid> versions,
        IRepository<GuideChunk, Guid> chunks,
        GuideChunkCache cache,
        IOptions<GuideRetrievalOptions> options)
    {
        _embeddingGenerator = embeddingGenerator;
        _guides = guides;
        _versions = versions;
        _chunks = chunks;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<GuideChunkMatch>> RetrieveAsync(
        string question, int? topK = null, double? similarityFloor = null, CancellationToken cancellationToken = default)
    {
        // The question rides the same local generator as the corpus (C1) - and its ModelId is
        // the space we may compare against: chunks from another model are invisible, not wrong.
        var embedded = (await _embeddingGenerator.GenerateAsync([question], cancellationToken: cancellationToken))[0];
        var questionVector = embedded.Vector.ToArray();
        var model = _embeddingGenerator.GetService<EmbeddingGeneratorMetadata>()?.DefaultModelId
                    ?? embedded.ModelId
                    ?? "unknown";

        var corpus = await _cache.GetOrLoadAsync(LoadServedCorpusAsync, _options.CacheTtl, cancellationToken);

        return corpus
            .Where(c => c.Model == model && c.Vector.Length == questionVector.Length)
            .Select(c => new GuideChunkMatch(
                c.ChunkId, c.GuideVersionId, c.ChunkNo, c.Text,
                VectorMath.CosineSimilarity(questionVector, c.Vector)))
            .Where(m => m.Score >= (similarityFloor ?? _options.SimilarityFloor))
            .OrderByDescending(m => m.Score)
            .Take(topK ?? _options.TopK)
            .ToList();
    }

    /// <summary>
    /// The SERVED corpus, mirroring GuideAppService's read model: active guides, and per
    /// (guide, language) only the latest published version - chunks of a superseded version
    /// stay in SQL (their citations stay resolvable) but stop being retrieval candidates.
    /// Three queries + in-memory shaping: honest for hundreds of chunks, revisited with VECTOR.
    /// </summary>
    private async Task<IReadOnlyList<CachedChunk>> LoadServedCorpusAsync()
    {
        var activeGuideIds = (await _guides.GetListAsync(g => g.IsActive)).Select(g => g.Id).ToHashSet();

        var servedVersionIds = (await _versions.GetListAsync(v => v.PublishedAt != null))
            .Where(v => activeGuideIds.Contains(v.GuideId))
            .GroupBy(v => new { v.GuideId, v.Language })
            .Select(g => g.OrderByDescending(v => v.VersionNo).First().Id)
            .ToHashSet();

        var chunks = await _chunks.GetListAsync(c => servedVersionIds.Contains(c.GuideVersionId));

        return chunks
            .Select(c => new CachedChunk(
                c.Id, c.GuideVersionId, c.ChunkNo, c.Text,
                EmbeddingConverter.ToFloats(c.Embedding), c.EmbeddingModel))
            .ToList();
    }
}
