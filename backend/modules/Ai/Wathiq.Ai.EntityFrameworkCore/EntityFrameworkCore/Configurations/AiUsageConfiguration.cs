using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Ai.Usage;

namespace Wathiq.Ai.EntityFrameworkCore.Configurations;

public class AiUsageConfiguration : IEntityTypeConfiguration<AiUsage>
{
    public void Configure(EntityTypeBuilder<AiUsage> b)
    {
        b.ToTable("Usage", AiDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Purpose).HasConversion<byte>();
        b.Property(x => x.Provider).HasMaxLength(AiUsageConsts.MaxProviderLength);
        b.Property(x => x.Model).HasMaxLength(AiUsageConsts.MaxModelLength);
        b.Property(x => x.PromptVersion).HasMaxLength(AiUsageConsts.MaxPromptVersionLength);

        // The daily-cap query (FR-AI-004): COUNT WHERE UserId = @me AND At >= @todayUtc.
        b.HasIndex(x => new { x.UserId, x.At }).HasDatabaseName("IX_Usage_UserId_At");
    }
}
