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
    // Reminders: EF for the schema migrator, Application so 2.7's permission definitions
    // are already in this graph the day they exist (the 1.7 admin-403 lesson).
    typeof(Wathiq.Reminders.EntityFrameworkCore.WathiqRemindersEntityFrameworkCoreModule),
    typeof(Wathiq.Reminders.WathiqRemindersApplicationModule),
    typeof(Wathiq.Ai.EntityFrameworkCore.WathiqAiEntityFrameworkCoreModule),
    typeof(Wathiq.Ai.WathiqAiApplicationModule),
    typeof(Wathiq.Guides.EntityFrameworkCore.WathiqGuidesEntityFrameworkCoreModule),
    typeof(Wathiq.Guides.WathiqGuidesApplicationModule),
    typeof(WathiqApplicationContractsModule)
)]
public class WathiqDbMigratorModule : AbpModule
{
}
