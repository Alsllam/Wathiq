using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Wathiq.Documents;

// App services, DTOs and the permission definition provider are picked up by convention:
// ApplicationService -> transient DI + auto API candidate; PermissionDefinitionProvider -> auto-added.
[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(WathiqDocumentsDomainModule)
)]
public class WathiqDocumentsApplicationModule : AbpModule
{
}
