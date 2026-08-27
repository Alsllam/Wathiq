using Wathiq.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Wathiq.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(WathiqEntityFrameworkCoreModule),
    typeof(Wathiq.Documents.EntityFrameworkCore.WathiqDocumentsEntityFrameworkCoreModule),
    // Brings the Documents permission definitions into the migrator so seeding can grant them.
    typeof(Wathiq.Documents.WathiqDocumentsApplicationModule),
    typeof(WathiqApplicationContractsModule)
)]
public class WathiqDbMigratorModule : AbpModule
{
}
