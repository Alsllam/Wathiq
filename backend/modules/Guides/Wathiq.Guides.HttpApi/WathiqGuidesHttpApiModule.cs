using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Wathiq.Guides;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(WathiqGuidesApplicationModule)
)]
public class WathiqGuidesHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Auto API controllers under /api/guides/*; endpoints arrive with 5.2 and 5.5.
            options.ConventionalControllers.Create(
                typeof(WathiqGuidesApplicationModule).Assembly,
                o => o.RootPath = "guides");
        });
    }
}
