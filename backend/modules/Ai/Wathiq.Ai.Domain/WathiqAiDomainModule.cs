using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Wathiq.Ai.Localization;
using Wathiq.Shared;

namespace Wathiq.Ai;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(WathiqSharedModule)
)]
public class WathiqAiDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<WathiqAiDomainModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<WathiqAiResource>("en")
                .AddVirtualJson("/Localization/WathiqAi");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Wathiq.Ai", typeof(WathiqAiResource));
        });
    }
}
