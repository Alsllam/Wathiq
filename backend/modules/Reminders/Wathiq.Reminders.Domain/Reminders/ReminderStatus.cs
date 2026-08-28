namespace Wathiq.Reminders.Reminders;

// Stored as tinyint (database.md). Never renumber: the values are persisted.
public enum ReminderStatus : byte
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3
}
