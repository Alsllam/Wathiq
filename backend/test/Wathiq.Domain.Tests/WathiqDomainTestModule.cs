using Volo.Abp.Modularity;

namespace Wathiq;

[DependsOn(
    typeof(WathiqDomainModule),
    typeof(WathiqTestBaseModule)
)]
public class WathiqDomainTestModule : AbpModule
{

}
