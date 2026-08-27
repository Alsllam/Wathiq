using Volo.Abp.Modularity;

namespace Wathiq;

[DependsOn(
    typeof(WathiqApplicationModule),
    typeof(WathiqDomainTestModule)
)]
public class WathiqApplicationTestModule : AbpModule
{

}
