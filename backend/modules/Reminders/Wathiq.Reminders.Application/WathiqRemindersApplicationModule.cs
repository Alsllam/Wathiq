using Volo.Abp.Application;
using Volo.Abp.Emailing;
using Volo.Abp.Modularity;

namespace Wathiq.Reminders;

// App services and permission definitions arrive in 2.7; the module exists now so every
// executable's graph (host, DbMigrator, test host) is wired before code lands in it.
[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpEmailingModule),
    typeof(WathiqRemindersDomainModule)
)]
public class WathiqRemindersApplicationModule : AbpModule
{
}
