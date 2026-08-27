using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Wathiq.Reminders;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(WathiqRemindersApplicationModule)
)]
public class WathiqRemindersHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Auto API controllers under /api/reminders/*; plural segment mapping arrives with
            // the app services in 2.7 (same pattern as Documents 1.7).
            options.ConventionalControllers.Create(
                typeof(WathiqRemindersApplicationModule).Assembly,
                o => o.RootPath = "reminders");
        });
    }
}
