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
            // Auto API controllers under /api/reminders/* (same recipe as Documents 1.7).
            options.ConventionalControllers.Create(
                typeof(WathiqRemindersApplicationModule).Assembly,
                o =>
                {
                    o.RootPath = "reminders";
                    o.UrlControllerNameNormalizer = context => context.ControllerName switch
                    {
                        // Singular on purpose: the rule is a singleton resource (one per user).
                        "ReminderRule" => "rule",
                        "Reminder" => "reminders",
                        _ => context.ControllerName
                    };
                });
        });
    }
}
