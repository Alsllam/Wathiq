using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Wathiq.Reminders.Reminders;

namespace Wathiq.Reminders.EntityFrameworkCore.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> b)
    {
        b.ToTable("Reminder", RemindersDbProperties.DbSchema);
        b.ConfigureByConvention();

        b.Property(x => x.Status).HasConversion<byte>();

        // The idempotency backbone (FR-REM-002): one row per document × offset, forever.
        // Rows are reused (Reschedule/Cancel), never deleted, so no soft-delete filter is needed.
        b.HasIndex(x => new { x.DocumentId, x.OffsetDays })
            .IsUnique()
            .HasDatabaseName("UQ_Reminder_DocumentId_OffsetDays");

        // The nightly job's scan: WHERE Status = Pending AND DueDate <= @today (2.5).
        b.HasIndex(x => new { x.Status, x.DueDate }).HasDatabaseName("IX_Reminder_Status_DueDate");
        b.HasIndex(x => x.UserId).HasDatabaseName("IX_Reminder_UserId");

        b.HasMany(x => x.DeliveryLogs).WithOne().HasForeignKey(l => l.ReminderId).OnDelete(DeleteBehavior.Cascade);
        // Unlike Attachment: NO AutoInclude. Logs are write-mostly; loading them on every
        // scheduler sync would drag history into memory for no reason. Adding to the unloaded
        // collection still inserts correctly - EF only needs the new child, not its siblings.
    }
}
