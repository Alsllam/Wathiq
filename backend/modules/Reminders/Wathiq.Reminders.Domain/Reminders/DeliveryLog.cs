using System;
using Volo.Abp.Domain.Entities;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.Reminders;

/// <summary>
/// One delivery attempt (FR-REM-005). Child of the Reminder aggregate: created only through
/// Reminder.RecordAttempt, never edited - it is an append-only log line with an id.
/// </summary>
public class DeliveryLog : Entity<Guid>
{
    public Guid ReminderId { get; private set; }
    public ReminderChannels Channel { get; private set; }   // exactly one flag per attempt
    public DateTime AttemptedAt { get; private set; }       // UTC (DB6)
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }

    private DeliveryLog()
    {
    }

    internal DeliveryLog(Guid id, Guid reminderId, ReminderChannels channel, DateTime attemptedAtUtc, bool succeeded, string? error)
        : base(id)
    {
        ReminderId = reminderId;
        Channel = channel;
        AttemptedAt = attemptedAtUtc;
        Succeeded = succeeded;
        // Truncate instead of throw: an SMTP stack trace must never abort writing the log line itself.
        Error = error?.Length > DeliveryLogConsts.MaxErrorLength ? error[..DeliveryLogConsts.MaxErrorLength] : error;
    }
}
