using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Wathiq.Ai;

// IChatClient routing and the usage-tracking decorator arrive in 3.3; the module exists now so
// every executable's graph is wired before code lands in it (the 2.1 discipline).
[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(WathiqAiDomainModule)
)]
public class WathiqAiApplicationModule : AbpModule
{
}
