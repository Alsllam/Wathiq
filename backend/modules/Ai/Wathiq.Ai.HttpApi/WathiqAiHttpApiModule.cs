using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Wathiq.Ai;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(WathiqAiApplicationModule)
)]
public class WathiqAiHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Auto API controllers under /api/ai/*; endpoints arrive with later steps.
            options.ConventionalControllers.Create(
                typeof(WathiqAiApplicationModule).Assembly,
                o => o.RootPath = "ai");
        });
    }
}
