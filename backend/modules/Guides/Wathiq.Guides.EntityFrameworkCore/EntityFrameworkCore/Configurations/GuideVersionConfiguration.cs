using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.EntityFrameworkCore.Configurations;

public class GuideVersionConfiguration : IEntityTypeConfiguration<GuideVersion>
{
    public void Configure(EntityTypeBuilder<GuideVersion> b)
    {
        b.ToTable("GuideVersion", GuidesDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Language).HasMaxLength(GuideConsts.LanguageLength).IsFixedLength();  // char(2) per DB doc
        b.Property(x => x.RequiredDocuments).HasMaxLength(GuideConsts.MaxRequiredDocumentsLength);
        b.Property(x => x.Fees).HasMaxLength(GuideConsts.MaxFeesLength);
        b.Property(x => x.Location).HasMaxLength(GuideConsts.MaxLocationLength);
        // DateOnly → SQL `date` by convention in EF 8+; nothing to configure, the migration proves it.

        b.HasIndex(x => x.GuideId).HasDatabaseName("IX_GuideVersion_GuideId");
        // The single authoring timeline per guide - two v3s would make citations ambiguous in prose.
        b.HasIndex(x => new { x.GuideId, x.VersionNo }).IsUnique().HasDatabaseName("IX_GuideVersion_GuideId_VersionNo");

        b.HasOne<Guide>().WithMany().HasForeignKey(x => x.GuideId).OnDelete(DeleteBehavior.Cascade);

        // Aggregate-internal collection, same shape as Document.Attachments: loads with the root.
        b.HasMany(x => x.Steps).WithOne().HasForeignKey(s => s.GuideVersionId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Steps).AutoInclude();
    }
}
