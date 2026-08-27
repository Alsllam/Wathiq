using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Wathiq.Documents;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(WathiqDocumentsApplicationModule)
)]
public class WathiqDocumentsHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Auto API controllers for this module's app services, grouped under /api/documents/*.
            options.ConventionalControllers.Create(
                typeof(WathiqDocumentsApplicationModule).Assembly,
                o => o.RootPath = "documents");
        });
    }
}
