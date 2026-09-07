using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.EntityFrameworkCore.Configurations;

public class GuideFeedbackConfiguration : IEntityTypeConfiguration<GuideFeedback>
{
    public void Configure(EntityTypeBuilder<GuideFeedback> b)
    {
        b.ToTable("GuideFeedback", GuidesDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Kind).HasConversion<byte>();   // tinyint per the DB doc
        b.Property(x => x.Comment).HasMaxLength(GuideConsts.MaxFeedbackCommentLength);

        b.HasIndex(x => x.GuideVersionId).HasDatabaseName("IX_GuideFeedback_GuideVersionId");

        b.HasOne<GuideVersion>().WithMany().HasForeignKey(x => x.GuideVersionId).OnDelete(DeleteBehavior.Cascade);
        // UserId is a REFERENCE to another module's user (no FK across schemas - DB2).
    }
}
