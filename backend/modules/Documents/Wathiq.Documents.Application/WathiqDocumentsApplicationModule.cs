using Volo.Abp.Application;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Wathiq.Documents;

// App services, DTOs and the permission definition provider are picked up by convention:
// ApplicationService -> transient DI + auto API candidate; PermissionDefinitionProvider -> auto-added.
[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpBackgroundJobsAbstractionsModule),   // IBackgroundJobManager + auto job-type discovery
    typeof(WathiqDocumentsDomainModule)
)]
public class WathiqDocumentsApplicationModule : AbpModule
{
}
