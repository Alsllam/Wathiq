using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Wathiq.Guides;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(WathiqGuidesDomainModule)
)]
public class WathiqGuidesApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Defaults live in the class; ops can tune floor/topK/TTL without a deploy (5.6 evals
        // will inform the numbers). No secrets here - just knobs.
        Configure<GuideRetrievalOptions>(context.Services.GetConfiguration().GetSection("Guides:Retrieval"));
    }
}
