using System;

namespace Wathiq.Reminders.Rules;

// [Flags]: a user can want Email AND Push; stored as one tinyint (database.md), tested with HasFlag.
[Flags]
public enum ReminderChannels : byte
{
    None = 0,
    Email = 1,
    Push = 2
}
