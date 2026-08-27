using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Wathiq.Documents;

// Application services + DTOs arrive in step 1.6; the module exists now so the dependency
// graph (Domain -> Application -> HttpApi) is fixed before any code lands in it.
[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(WathiqDocumentsDomainModule)
)]
public class WathiqDocumentsApplicationModule : AbpModule
{
}
