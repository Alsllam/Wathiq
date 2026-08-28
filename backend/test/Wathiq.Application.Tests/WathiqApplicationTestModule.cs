using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Wathiq;

[DependsOn(
    typeof(WathiqApplicationModule),
    typeof(Wathiq.Documents.WathiqDocumentsApplicationModule),
    typeof(Wathiq.Reminders.WathiqRemindersApplicationModule),
    typeof(WathiqDomainTestModule)
)]
public class WathiqApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Singleton so a test can inspect what "was sent"; the job discovers it as IReminderChannel.
        context.Services.AddSingleton<Reminders.FakeReminderChannel>();
        context.Services.AddSingleton<Reminders.Reminders.IReminderChannel>(
            sp => sp.GetRequiredService<Reminders.FakeReminderChannel>());
    }
}
