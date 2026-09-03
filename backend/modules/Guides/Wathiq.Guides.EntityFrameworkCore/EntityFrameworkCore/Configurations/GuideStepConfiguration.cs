using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.EntityFrameworkCore.Configurations;

public class GuideStepConfiguration : IEntityTypeConfiguration<GuideStep>
{
    public void Configure(EntityTypeBuilder<GuideStep> b)
    {
        b.ToTable("GuideStep", GuidesDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Text).HasMaxLength(GuideConsts.MaxStepTextLength);

        b.HasIndex(x => new { x.GuideVersionId, x.StepNo }).IsUnique()
            .HasDatabaseName("IX_GuideStep_GuideVersionId_StepNo");
    }
}
