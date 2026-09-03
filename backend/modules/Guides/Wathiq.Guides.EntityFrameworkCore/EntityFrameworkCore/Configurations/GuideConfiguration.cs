using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.EntityFrameworkCore.Configurations;

public class GuideConfiguration : IEntityTypeConfiguration<Guide>
{
    public void Configure(EntityTypeBuilder<Guide> b)
    {
        b.ToTable("Guide", GuidesDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Slug).HasMaxLength(GuideConsts.MaxSlugLength);
        b.Property(x => x.TitleAr).HasMaxLength(GuideConsts.MaxTitleLength);
        b.Property(x => x.TitleEn).HasMaxLength(GuideConsts.MaxTitleLength);

        b.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Guide_Slug");

        // NoAction, not Cascade: GuideVersion.GuideId already cascades Guide→Version; a second
        // cascade path back (Version→Guide.PublishedVersionId) would be rejected by SQL Server.
        b.HasOne<GuideVersion>().WithMany().HasForeignKey(x => x.PublishedVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
