using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Reminders.Reminders;

namespace Wathiq.Reminders.EntityFrameworkCore.Configurations;

public class DeliveryLogConfiguration : IEntityTypeConfiguration<DeliveryLog>
{
    public void Configure(EntityTypeBuilder<DeliveryLog> b)
    {
        b.ToTable("DeliveryLog", RemindersDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Channel).HasConversion<byte>();
        b.Property(x => x.Error).HasMaxLength(DeliveryLogConsts.MaxErrorLength);

        b.HasIndex(x => x.ReminderId).HasDatabaseName("IX_DeliveryLog_ReminderId");
    }
}
