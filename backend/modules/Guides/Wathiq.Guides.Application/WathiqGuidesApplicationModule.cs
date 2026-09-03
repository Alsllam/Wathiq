using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Wathiq.Guides;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(WathiqGuidesDomainModule)
)]
public class WathiqGuidesApplicationModule : AbpModule
{
    // App services arrive with 5.2 (authoring + public reading). The permission definition
    // provider in Permissions/ is auto-discovered - no registration line needed.
}
