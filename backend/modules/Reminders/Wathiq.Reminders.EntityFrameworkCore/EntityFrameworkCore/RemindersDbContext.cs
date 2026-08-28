using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.EntityFrameworkCore;

// Second module DbContext (ADR-001): knows only reminders.* - Document rows are reachable
// solely by id, so a join to documents.* cannot even be written here.
[ConnectionStringName(RemindersDbProperties.ConnectionStringName)]
public class RemindersDbContext : AbpDbContext<RemindersDbContext>
{
    public DbSet<ReminderRule> ReminderRules { get; set; } = default!;
    public DbSet<Reminder> Reminders { get; set; } = default!;
    // No DbSet<DeliveryLog>: reached through Reminder only (aggregate boundary).

    public RemindersDbContext(DbContextOptions<RemindersDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(RemindersDbProperties.DbSchema);

        builder.ApplyConfigurationsFromAssembly(typeof(RemindersDbContext).Assembly);
    }
}
