using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Wathiq.Guides.Localization;
using Wathiq.Shared;

namespace Wathiq.Guides;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(WathiqSharedModule)
)]
public class WathiqGuidesDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<WathiqGuidesDomainModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<WathiqGuidesResource>("en")
                .AddVirtualJson("/Localization/WathiqGuides");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Wathiq.Guides", typeof(WathiqGuidesResource));
        });
    }
}
