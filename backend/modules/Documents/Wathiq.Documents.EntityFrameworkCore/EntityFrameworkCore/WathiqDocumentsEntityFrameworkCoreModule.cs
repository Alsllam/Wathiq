using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;

namespace Wathiq.Documents.EntityFrameworkCore;

[DependsOn(
    typeof(WathiqDocumentsDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule)
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

        Configure<AbpDbContextOptions>(options =>
        {
            // Scoped to this context only; the host's global UseSqlServer() still applies to WathiqDbContext.
            options.Configure<DocumentsDbContext>(ctx => ctx.UseSqlServer(ConfigureSqlServer));
        });
    }

    // Shared by the runtime module and the design-time factory so both agree on the history table.
    public static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql)
    {
        // Each module tracks its own migrations: documents.__EFMigrationsHistory, not the dbo one.
        sql.MigrationsHistoryTable("__EFMigrationsHistory", DocumentsDbProperties.DbSchema);
    }
}
