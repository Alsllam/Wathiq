using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.Reminders;

/// <summary>
/// One scheduled reminder: document × offset (FR-REM-001). The pair is database-unique
/// (UQ_Reminder_DocumentId_OffsetDays), so rescheduling REUSES this row via Reschedule()
/// instead of delete-and-insert - that reuse is the backbone of FR-REM-002 idempotency.
/// </summary>
public class Reminder : FullAuditedAggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid DocumentId { get; private set; }   // -> documents.Document by id only, no FK (DB2)
    public int OffsetDays { get; private set; }
    public DateOnly DueDate { get; private set; }
    public ReminderStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }  // UTC

    private readonly List<DeliveryLog> _deliveryLogs = new();
    public IReadOnlyCollection<DeliveryLog> DeliveryLogs => _deliveryLogs.AsReadOnly();

    private Reminder()
    {
    }

    public Reminder(Guid id, Guid userId, Guid documentId, int offsetDays, DateOnly dueDate)
        : base(id)
    {
        UserId = userId;
        DocumentId = documentId;
        OffsetDays = offsetDays;
        DueDate = dueDate;
        Status = ReminderStatus.Pending;
    }

    /// <summary>Re-arms the row for a new due date (expiry changed / document renewed). SentAt clears; history stays in DeliveryLogs.</summary>
    public Reminder Reschedule(DateOnly newDueDate)
    {
        DueDate = newDueDate;
        Status = ReminderStatus.Pending;
        SentAt = null;
        return this;
    }

    /// <summary>A sent reminder stays Sent - cancelling only stops ones that haven't gone out.</summary>
    public Reminder Cancel()
    {
        if (Status is ReminderStatus.Pending or ReminderStatus.Failed)
        {
            Status = ReminderStatus.Cancelled;
        }

        return this;
    }

    /// <summary>The only way a DeliveryLog is born; also the Pending -> Sent/Failed transition (FR-REM-002/005).
    /// The id comes from the caller (GuidGenerator - sequential, DB3), same as Document.AddAttachment.</summary>
    public DeliveryLog RecordAttempt(Guid logId, ReminderChannels channel, DateTime attemptedAtUtc, bool succeeded, string? error = null)
    {
        var log = new DeliveryLog(logId, Id, channel, attemptedAtUtc, succeeded, error);
        _deliveryLogs.Add(log);

        if (succeeded)
        {
            Status = ReminderStatus.Sent;
            SentAt = attemptedAtUtc;
        }
        else
        {
            Status = ReminderStatus.Failed;
        }

        return log;
    }
}
