using Volo.Abp.Domain;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Wathiq.Reminders.Localization;
using Wathiq.Shared;

namespace Wathiq.Reminders;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(WathiqSharedModule)
)]
public class WathiqRemindersDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<WathiqRemindersDomainModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<WathiqRemindersResource>("en")
                .AddVirtualJson("/Localization/WathiqReminders");
        });

        // BusinessException("Wathiq.Reminders:...") -> localized text, same wiring as Documents.
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Wathiq.Reminders", typeof(WathiqRemindersResource));
        });
    }
}
