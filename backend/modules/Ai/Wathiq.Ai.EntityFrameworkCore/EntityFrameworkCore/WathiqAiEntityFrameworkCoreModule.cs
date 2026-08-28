using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Wathiq.Ai.EntityFrameworkCore;

[DependsOn(
    typeof(WathiqAiDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class WathiqAiEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AiDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: false);
        });

        // No provider here (composition-root rule): the host and the Sqlite test host each decide.
    }

    public static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql)
    {
        sql.MigrationsHistoryTable("__EFMigrationsHistory", AiDbProperties.DbSchema);
    }
}
