using System.Threading.Tasks;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.Reminders;

/// <summary>
/// One delivery medium (FR-REM-003). The dispatch job discovers implementations from DI and
/// matches them to the user's channel flags - adding push in Phase 6 is a new implementation,
/// not a job change. Implementations throw on failure; the job turns that into a Failed row.
/// </summary>
public interface IReminderChannel
{
    ReminderChannels Channel { get; }

    Task SendAsync(Reminder reminder, ReminderRule rule);
}
