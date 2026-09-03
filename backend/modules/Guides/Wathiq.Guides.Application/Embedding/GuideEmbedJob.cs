using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.Embedding;

/// <summary>
/// Chunks a published version and embeds every chunk (FR-GDE-003). Injects the FRAMEWORK
/// abstraction IEmbeddingGenerator - the Ai module decides who serves it (local bge-m3, enforced
/// at boot), this module never learns. Idempotent by delete-and-rebuild: chunks are derived
/// data with no identity worth preserving, so a retry after a crash regenerates cleanly.
/// </summary>
// ITransientDependency: DI registration AND ABP job discovery (the 3.5 lesson, at the line).
public class GuideEmbedJob : AsyncBackgroundJob<GuideEmbedArgs>, IUnitOfWorkEnabled, Volo.Abp.DependencyInjection.ITransientDependency
{
    private readonly IRepository<GuideVersion, Guid> _versions;
    private readonly IRepository<GuideChunk, Guid> _chunks;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<GuideEmbedJob> _logger;

    public GuideEmbedJob(
        IRepository<GuideVersion, Guid> versions,
        IRepository<GuideChunk, Guid> chunks,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IGuidGenerator guidGenerator,
        ILogger<GuideEmbedJob> logger)
    {
        _versions = versions;
        _chunks = chunks;
        _embeddingGenerator = embeddingGenerator;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public override async Task ExecuteAsync(GuideEmbedArgs args)
    {
        // FindAsync: deleted-between-enqueue-and-run is a non-event for a job, not a retry storm.
        var version = await _versions.FindAsync(args.GuideVersionId);
        if (version is null || !version.IsPublished)
        {
            _logger.LogInformation("Embed skipped: version {VersionId} missing or unpublished.", args.GuideVersionId);
            return;
        }

        var drafts = GuideChunker.Chunk(
            version.BodyMarkdown,
            version.Steps.OrderBy(s => s.StepNo).Select(s => s.Text),
            version.RequiredDocuments, version.Fees, version.Location);

        // One batched call for all chunks of the version - the generator's unit of work.
        var embeddings = await _embeddingGenerator.GenerateAsync(drafts.Select(d => d.Text).ToList());

        // The generator knows which model it runs; storing it makes stale vectors detectable
        // (a model swap re-embeds because 5.4 filters on this column).
        var model = _embeddingGenerator.GetService<EmbeddingGeneratorMetadata>()?.DefaultModelId
                    ?? embeddings.FirstOrDefault()?.ModelId
                    ?? "unknown";

        await _chunks.DeleteAsync(c => c.GuideVersionId == version.Id);   // rebuild, don't merge

        var rows = drafts.Zip(embeddings, (draft, embedding) => new GuideChunk(
            _guidGenerator.Create(), version.Id, draft.ChunkNo, draft.Text,
            EmbeddingConverter.ToBytes(embedding.Vector.Span), model, draft.TokenCount)).ToList();

        await _chunks.InsertManyAsync(rows);

        _logger.LogInformation("Embedded version {VersionId}: {Count} chunks via {Model}.", version.Id, rows.Count, model);
    }
}
