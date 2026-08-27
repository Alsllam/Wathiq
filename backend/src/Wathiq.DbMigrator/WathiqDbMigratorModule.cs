using Wathiq.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Wathiq.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(WathiqEntityFrameworkCoreModule),
    typeof(WathiqApplicationContractsModule)
)]
public class WathiqDbMigratorModule : AbpModule
{
}
