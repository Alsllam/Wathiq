using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Wathiq.Shared;

namespace Wathiq.Documents;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(WathiqSharedModule)
)]
public class WathiqDocumentsDomainModule : AbpModule
{
}
