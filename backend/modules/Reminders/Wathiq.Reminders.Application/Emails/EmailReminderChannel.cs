using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.Localization;
using Wathiq.Reminders.Localization;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;
using Wathiq.Shared.Users;

namespace Wathiq.Reminders.Emails;

/// <summary>
/// FR-REM-003's email medium. Sends BOTH languages in one mail (no per-user culture setting
/// yet): Arabic first as an RTL block, English after - the mail is readable whoever opens it.
/// Content is deliberately generic (date + days left, no document details): the reminder row
/// only knows the DocumentId, and pulling names across the module boundary is not this step's job.
/// </summary>
public class EmailReminderChannel : IReminderChannel, ITransientDependency
{
    private readonly IEmailSender _emailSender;
    private readonly IUserContactResolver _contacts;
    private readonly IStringLocalizer<WathiqRemindersResource> _l;

    public EmailReminderChannel(
        IEmailSender emailSender,
        IUserContactResolver contacts,
        IStringLocalizer<WathiqRemindersResource> localizer)
    {
        _emailSender = emailSender;
        _contacts = contacts;
        _l = localizer;
    }

    public ReminderChannels Channel => ReminderChannels.Email;

    public async Task SendAsync(Reminder reminder, ReminderRule rule)
    {
        var contact = await _contacts.FindAsync(reminder.UserId);
        if (contact?.Email == null)
        {
            // Throw, don't skip silently: the job records this as a Failed attempt (FR-REM-005),
            // which is the only place an operator would ever see "user has no email".
            throw new BusinessException(RemindersErrorCodes.NoEmailAddress)
                .WithData("UserId", reminder.UserId);
        }

        var expiry = reminder.DueDate.AddDays(reminder.OffsetDays);
        var expiryText = expiry.ToString("yyyy-MM-dd");

        // CultureHelper.Use pins each string's language explicitly - the job thread's culture is
        // nobody's culture, so relying on CurrentUICulture here would be a bug.
        string arSubject, arBody, enSubject, enBody;
        using (CultureHelper.Use("ar"))
        {
            arSubject = _l["Email:ReminderSubject", expiryText];
            arBody = _l["Email:ReminderBody", expiryText, reminder.OffsetDays];
        }
        using (CultureHelper.Use("en"))
        {
            enSubject = _l["Email:ReminderSubject", expiryText];
            enBody = _l["Email:ReminderBody", expiryText, reminder.OffsetDays];
        }

        // Logical/RTL-safe by construction: each block carries its own dir; no left/right CSS.
        var body =
            $"""
             <div dir="rtl" style="font-family:sans-serif">{arBody}</div>
             <hr/>
             <div dir="ltr" style="font-family:sans-serif">{enBody}</div>
             """;

        await _emailSender.SendAsync(contact.Email, $"{arSubject} | {enSubject}", body, isBodyHtml: true);
    }
}
