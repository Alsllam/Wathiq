using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Wathiq.Reminders.EntityFrameworkCore;

[DependsOn(
    typeof(WathiqRemindersDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class WathiqRemindersEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<RemindersDbContext>(options =>
        {
            // Repositories only for aggregate roots: child entities are reached through their root.
            options.AddDefaultRepositories(includeAllEntities: false);
        });

        // No provider here (composition-root rule learned in 1.3): the host picks the DBMS
        // and calls ConfigureSqlServer for this context; the Sqlite test host configures its own.
    }

    // Shared by the host module and the design-time factory so both agree on the history table.
    public static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql)
    {
        // Each module tracks its own migrations: reminders.__EFMigrationsHistory, not the dbo one.
        sql.MigrationsHistoryTable("__EFMigrationsHistory", RemindersDbProperties.DbSchema);
    }
}
