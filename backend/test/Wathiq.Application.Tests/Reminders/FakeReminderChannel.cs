using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders;

/// <summary>Test double for 2.6's email channel: records every send, throws on demand.</summary>
public class FakeReminderChannel : IReminderChannel
{
    public ReminderChannels Channel => ReminderChannels.Email;

    public List<Guid> SentReminderIds { get; } = [];
    public Func<Reminder, bool>? FailWhen { get; set; }

    public Task SendAsync(Reminder reminder, ReminderRule rule)
    {
        if (FailWhen?.Invoke(reminder) == true)
        {
            throw new InvalidOperationException("SMTP said no (simulated). " + new string('x', 600));
        }

        SentReminderIds.Add(reminder.Id);
        return Task.CompletedTask;
    }
}
