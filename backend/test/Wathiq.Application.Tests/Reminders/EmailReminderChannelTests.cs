using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Wathiq.Reminders.Emails;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;
using Wathiq.Shared.Users;
using Xunit;

namespace Wathiq.Reminders;

public abstract class EmailReminderChannelTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly EmailReminderChannel _channel;
    private readonly RecordingEmailSender _emailSender;
    private readonly FakeUserContactResolver _contacts;

    protected EmailReminderChannelTests()
    {
        _channel = GetRequiredService<EmailReminderChannel>();
        _emailSender = GetRequiredService<RecordingEmailSender>();
        _contacts = GetRequiredService<FakeUserContactResolver>();
    }

    private static Reminder NewReminder(Guid userId) =>
        new(Guid.NewGuid(), userId, Guid.NewGuid(), offsetDays: 7, dueDate: new DateOnly(2026, 9, 1));

    private static ReminderRule NewRule(Guid userId) =>
        new(Guid.NewGuid(), userId, ReminderOffsets.Default, ReminderChannels.Email, ReminderRuleConsts.DefaultTimeZoneId);

    [Fact]
    public async Task Sends_A_Bilingual_Mail_To_The_Resolved_Address()
    {
        var userId = Guid.NewGuid();

        await _channel.SendAsync(NewReminder(userId), NewRule(userId));

        var mail = _emailSender.Sent.ShouldHaveSingleItem();
        mail.To.ShouldBe($"{userId:N}@test.local");
        mail.Subject.ShouldContain("2026-09-08");        // due + offset = the real expiry date
        mail.Subject.ShouldContain("تذكير");             // Arabic half
        mail.Subject.ShouldContain("Reminder");          // English half
        mail.Body.ShouldContain("dir=\"rtl\"");          // RTL-safe by construction
    }

    [Fact]
    public async Task No_Email_Address_Throws_So_The_Job_Records_A_Failure()
    {
        var userId = Guid.NewGuid();
        _contacts.Contacts[userId] = new UserContact(Email: null, DisplayName: "بدون بريد");

        var ex = await Should.ThrowAsync<BusinessException>(
            () => _channel.SendAsync(NewReminder(userId), NewRule(userId)));

        ex.Code.ShouldBe(RemindersErrorCodes.NoEmailAddress);
        _emailSender.Sent.ShouldBeEmpty();
    }
}
