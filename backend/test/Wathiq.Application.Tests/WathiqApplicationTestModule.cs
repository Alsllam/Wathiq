using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace Wathiq;

[DependsOn(
    typeof(WathiqApplicationModule),
    typeof(Wathiq.Documents.WathiqDocumentsApplicationModule),
    typeof(Wathiq.Reminders.WathiqRemindersApplicationModule),
    typeof(Wathiq.Ai.WathiqAiApplicationModule),
    typeof(WathiqDomainTestModule)
)]
public class WathiqApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // The real EmailReminderChannel registered itself as IReminderChannel (2.6); with it AND
        // the fake, every dispatch would double-send. Tests keep exactly one channel: the fake.
        // (EmailReminderChannel stays resolvable as itself for its own direct test.)
        context.Services.RemoveAll<Reminders.Reminders.IReminderChannel>();
        context.Services.AddSingleton<Reminders.FakeReminderChannel>();
        context.Services.AddSingleton<Reminders.Reminders.IReminderChannel>(
            sp => sp.GetRequiredService<Reminders.FakeReminderChannel>());

        // Identity is not in this graph; contact lookup and SMTP get scripted stand-ins.
        context.Services.AddSingleton<Reminders.FakeUserContactResolver>();
        context.Services.Replace(ServiceDescriptor.Singleton<Wathiq.Shared.Users.IUserContactResolver>(
            sp => sp.GetRequiredService<Reminders.FakeUserContactResolver>()));
        context.Services.AddSingleton<Reminders.RecordingEmailSender>();
        context.Services.Replace(ServiceDescriptor.Singleton<Volo.Abp.Emailing.IEmailSender>(
            sp => sp.GetRequiredService<Reminders.RecordingEmailSender>()));

        // 3.5: the Tesseract adapter lives in the host, outside this graph - the port MUST get a
        // fake here or the OCR job can't resolve. The queue is replaced by a recorder so tests
        // assert "enqueued", then invoke the job directly.
        context.Services.AddSingleton<Documents.FakeOcrService>();
        context.Services.AddSingleton<Wathiq.Shared.Ocr.IOcrService>(
            sp => sp.GetRequiredService<Documents.FakeOcrService>());
        context.Services.AddSingleton<Documents.RecordingBackgroundJobManager>();
        context.Services.Replace(ServiceDescriptor.Singleton<Volo.Abp.BackgroundJobs.IBackgroundJobManager>(
            sp => sp.GetRequiredService<Documents.RecordingBackgroundJobManager>()));
    }
}
