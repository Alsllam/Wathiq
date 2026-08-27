using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.EntityFrameworkCore.Configurations;

public class ReminderRuleConfiguration : IEntityTypeConfiguration<ReminderRule>
{
    public void Configure(EntityTypeBuilder<ReminderRule> b)
    {
        b.ToTable("ReminderRule", RemindersDbProperties.DbSchema);
        b.ConfigureByConvention();

        // Value conversion, not an owned type: the whole value object round-trips through ONE
        // scalar column. The comparer is mandatory - without it EF compares object references
        // and would either miss changes or rewrite the row on every save.
        b.Property(x => x.Offsets)
            .HasColumnName("OffsetsDays")
            .HasMaxLength(ReminderRuleConsts.MaxOffsetsDaysLength)
            .HasConversion(o => o.ToCsv(), csv => ReminderOffsets.FromCsv(csv))
            .Metadata.SetValueComparer(new ValueComparer<ReminderOffsets>(
                (a, c) => (a == null && c == null) || (a != null && a.Equals(c)),
                o => o.GetHashCode(),
                o => ReminderOffsets.FromCsv(o.ToCsv())));

        b.Property(x => x.Channels).HasConversion<byte>();
        b.Property(x => x.TimeZoneId).HasMaxLength(ReminderRuleConsts.MaxTimeZoneIdLength);
        // TimeOnly? maps to SQL `time` natively (EF Core 8+); nothing to configure.

        // One rule per user, enforced by the database, not just the manager.
        b.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("UQ_ReminderRule_UserId");
    }
}
