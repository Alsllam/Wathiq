using Volo.Abp.Modularity;

namespace Wathiq;

[DependsOn(
    typeof(WathiqApplicationModule),
    typeof(Wathiq.Documents.WathiqDocumentsApplicationModule),
    typeof(WathiqDomainTestModule)
)]
public class WathiqApplicationTestModule : AbpModule
{

}
