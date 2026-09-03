using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.EntityFrameworkCore.Configurations;

public class GuideChunkConfiguration : IEntityTypeConfiguration<GuideChunk>
{
    public void Configure(EntityTypeBuilder<GuideChunk> b)
    {
        b.ToTable("GuideChunk", GuidesDbProperties.DbSchema);
        b.ConfigureByConvention();

        // varbinary(4096), not (max): 1024 float32s exactly - the column width IS the model
        // contract (bge-m3, D2). The DB doc notes VECTOR(1024) once SQL Server ships it.
        b.Property(x => x.Embedding).HasMaxLength(GuideConsts.EmbeddingByteLength);
        b.Property(x => x.EmbeddingModel).HasMaxLength(GuideConsts.MaxEmbeddingModelLength);

        b.HasIndex(x => x.GuideVersionId).HasDatabaseName("IX_GuideChunk_GuideVersionId");
        b.HasIndex(x => new { x.GuideVersionId, x.ChunkNo }).IsUnique()
            .HasDatabaseName("IX_GuideChunk_GuideVersionId_ChunkNo");

        b.HasOne<GuideVersion>().WithMany().HasForeignKey(x => x.GuideVersionId).OnDelete(DeleteBehavior.Cascade);
    }
}
