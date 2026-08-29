using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Documents.Extraction;

namespace Wathiq.Documents.EntityFrameworkCore.Configurations;

public class ExtractionResultConfiguration : IEntityTypeConfiguration<ExtractionResult>
{
    public void Configure(EntityTypeBuilder<ExtractionResult> b)
    {
        b.ToTable("ExtractionResult", DocumentsDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Provider).IsRequired().HasMaxLength(ExtractionResultConsts.MaxProviderLength);
        b.Property(x => x.Model).IsRequired().HasMaxLength(ExtractionResultConsts.MaxModelLength);
        b.Property(x => x.PromptVersion).IsRequired().HasMaxLength(ExtractionResultConsts.MaxPromptVersionLength);
        b.Property(x => x.RawJson).IsRequired();
        // decimal(4,3): 0.000-1.000 - the doc's planned precision, plenty for a self-estimate.
        b.Property(x => x.Confidence).HasPrecision(4, 3);

        // Same-module FK (the module rule forbids only cross-module ones). Cascade: a deleted
        // attachment leaves no orphaned extraction PII behind (P8 hygiene).
        b.HasOne<Documents.Attachment>().WithMany()
            .HasForeignKey(x => x.AttachmentId).OnDelete(DeleteBehavior.Cascade);
        // 3.7's query: "results for this attachment, newest first".
        b.HasIndex(x => x.AttachmentId).HasDatabaseName("IX_ExtractionResult_AttachmentId");
    }
}
