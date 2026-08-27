using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Wathiq.Reminders.EntityFrameworkCore;

// Second module DbContext (ADR-001): knows only reminders.* - Document rows are reachable
// solely by id, so a join to documents.* cannot even be written here.
[ConnectionStringName(RemindersDbProperties.ConnectionStringName)]
public class RemindersDbContext : AbpDbContext<RemindersDbContext>
{
    // DbSets arrive with the entities: ReminderRule (2.2), Reminder + DeliveryLog (2.3).

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
