using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Guides.Guides;

/// <summary>
/// One retrievable slice of a published version (database.md E-GuideChunk). Derived data:
/// regenerated wholesale by the embed job, never edited. The FK targets a GuideVersion - not a
/// Guide - so a citation of this chunk stays true even after the guide is re-authored (the
/// version is immutable; the chunk inherits that stability).
/// </summary>
public class GuideChunk : CreationAuditedAggregateRoot<Guid>
{
    public Guid GuideVersionId { get; private set; }
    public int ChunkNo { get; private set; }
    public string Text { get; private set; } = default!;
    /// <summary>1024 × float32 little-endian (bge-m3, D2). varbinary(4096); VECTOR(1024) when SQL Server ships it.</summary>
    public byte[] Embedding { get; private set; } = default!;
    /// <summary>Which model produced the vector - vectors from different models don't share a space, so 5.4 must filter by this.</summary>
    public string EmbeddingModel { get; private set; } = default!;
    public int TokenCount { get; private set; }

    private GuideChunk()
    {
    }

    public GuideChunk(Guid id, Guid guideVersionId, int chunkNo, string text, byte[] embedding, string embeddingModel, int tokenCount)
        : base(id)
    {
        GuideVersionId = guideVersionId;
        ChunkNo = chunkNo;
        Text = Check.NotNullOrWhiteSpace(text, nameof(text));
        Embedding = Check.NotNull(embedding, nameof(embedding));
        EmbeddingModel = Check.NotNullOrWhiteSpace(embeddingModel, nameof(embeddingModel), GuideConsts.MaxEmbeddingModelLength);
        TokenCount = tokenCount;
    }
}
