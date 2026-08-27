using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Wathiq.Documents.Localization;
using Wathiq.Shared;

namespace Wathiq.Documents;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(WathiqSharedModule)
)]
public class WathiqDocumentsDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<WathiqDocumentsDomainModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<WathiqDocumentsResource>("en")
                .AddVirtualJson("/Localization/WathiqDocuments");
        });

        // BusinessException("Wathiq.Documents:...") -> text from WathiqDocumentsResource in the caller's language.
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Wathiq.Documents", typeof(WathiqDocumentsResource));
        });
    }
}
