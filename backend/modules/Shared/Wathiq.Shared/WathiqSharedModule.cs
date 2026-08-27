using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Wathiq.Shared.Files;
using Wathiq.Shared.Localization;

namespace Wathiq.Shared;

// No [DependsOn] on another business module: Shared is a leaf — every other module may depend on
// it, it depends on nothing but the ABP kernel (Volo.Abp.Core), keeping the module graph acyclic.
public class WathiqSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<WathiqSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<WathiqSharedResource>("en")
                .AddVirtualJson("/Localization/WathiqShared");
        });

        // Lets BusinessException(WathiqSharedErrorCodes.*) messages resolve from WathiqSharedResource.
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Wathiq.Shared", typeof(WathiqSharedResource));
        });

        Configure<FileStoreOptions>(configuration.GetSection("FileStore"));
    }
}
