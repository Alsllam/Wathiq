using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Volo.Abp.Emailing;

namespace Wathiq.Reminders;

/// <summary>Captures outgoing mail instead of talking SMTP - the seam IEmailSender exists for.</summary>
public class RecordingEmailSender : IEmailSender
{
    public record Mail(string To, string Subject, string Body);

    public List<Mail> Sent { get; } = [];

    public Task SendAsync(string to, string subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        Sent.Add(new Mail(to, subject, body ?? string.Empty));
        return Task.CompletedTask;
    }

    public Task SendAsync(string from, string to, string subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
        => SendAsync(to, subject, body, isBodyHtml, additionalEmailSendingArgs);

    public Task SendAsync(MailMessage mail, bool normalize = true)
    {
        Sent.Add(new Mail(mail.To.ToString(), mail.Subject ?? string.Empty, mail.Body ?? string.Empty));
        return Task.CompletedTask;
    }

    public Task QueueAsync(string to, string subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
        => SendAsync(to, subject, body, isBodyHtml, additionalEmailSendingArgs);

    public Task QueueAsync(string from, string to, string subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
        => SendAsync(to, subject, body, isBodyHtml, additionalEmailSendingArgs);
}
