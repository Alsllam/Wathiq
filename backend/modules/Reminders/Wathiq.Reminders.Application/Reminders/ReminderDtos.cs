using System;

namespace Wathiq.Reminders.Reminders;

public class ReminderDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int OffsetDays { get; set; }
    public DateOnly DueDate { get; set; }
    /// <summary>DueDate + OffsetDays: saves every client the same little sum.</summary>
    public DateOnly ExpiryDate { get; set; }
    public ReminderStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
}
