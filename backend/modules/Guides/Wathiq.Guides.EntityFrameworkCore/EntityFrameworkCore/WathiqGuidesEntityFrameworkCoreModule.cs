using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Wathiq.Guides.EntityFrameworkCore;

[DependsOn(
    typeof(WathiqGuidesDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class WathiqGuidesEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<GuidesDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: false);
        });

        // No provider here (composition-root rule): the host and the Sqlite test host each decide.
    }

    public static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql)
    {
        sql.MigrationsHistoryTable("__EFMigrationsHistory", GuidesDbProperties.DbSchema);
    }
}
