using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Wathiq.Documents.EntityFrameworkCore;

[DependsOn(
    typeof(WathiqDocumentsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class WathiqDocumentsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<DocumentsDbContext>(options =>
        {
            // Repositories only for aggregate roots: child entities are reached through their root.
            options.AddDefaultRepositories(includeAllEntities: false);
        });

        // Deliberately no provider here. A per-context AbpDbContextOptions.Configure<DocumentsDbContext>
        // *replaces* the global default, which broke the Sqlite test host; the DBMS is the host's
        // decision (composition root), and it calls ConfigureSqlServer for this context.
    }

    // Shared by the host module and the design-time factory so both agree on the history table.
    public static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql)
    {
        // Each module tracks its own migrations: documents.__EFMigrationsHistory, not the dbo one.
        sql.MigrationsHistoryTable("__EFMigrationsHistory", DocumentsDbProperties.DbSchema);
    }
}
