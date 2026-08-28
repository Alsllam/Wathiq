using System;
using Shouldly;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;
using Xunit;

namespace Wathiq.Reminders;

public class ReminderTests
{
    private static Reminder NewReminder() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), offsetDays: 30, dueDate: new DateOnly(2026, 9, 26));

    [Fact]
    public void Successful_Attempt_Marks_Sent_And_Logs()
    {
        var reminder = NewReminder();
        var at = new DateTime(2026, 9, 26, 3, 0, 0, DateTimeKind.Utc);

        reminder.RecordAttempt(Guid.NewGuid(), ReminderChannels.Email, at, succeeded: true);

        reminder.Status.ShouldBe(ReminderStatus.Sent);
        reminder.SentAt.ShouldBe(at);
        reminder.DeliveryLogs.ShouldHaveSingleItem().Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Failed_Attempt_Marks_Failed_And_Truncates_The_Error()
    {
        var reminder = NewReminder();

        var log = reminder.RecordAttempt(Guid.NewGuid(), ReminderChannels.Email,
            DateTime.UtcNow, succeeded: false, error: new string('x', 2000));

        reminder.Status.ShouldBe(ReminderStatus.Failed);
        reminder.SentAt.ShouldBeNull();
        log.Error!.Length.ShouldBe(DeliveryLogConsts.MaxErrorLength); // truncated, not thrown
    }

    [Fact]
    public void Cancel_Never_Touches_A_Sent_Reminder()
    {
        var sent = NewReminder();
        sent.RecordAttempt(Guid.NewGuid(), ReminderChannels.Email, DateTime.UtcNow, succeeded: true);

        sent.Cancel().Status.ShouldBe(ReminderStatus.Sent);      // history is immutable
        NewReminder().Cancel().Status.ShouldBe(ReminderStatus.Cancelled);
    }

    [Fact]
    public void Reschedule_Rearms_The_Row_For_A_New_Date()
    {
        var reminder = NewReminder();
        reminder.RecordAttempt(Guid.NewGuid(), ReminderChannels.Email, DateTime.UtcNow, succeeded: true);

        reminder.Reschedule(new DateOnly(2031, 9, 26));

        reminder.Status.ShouldBe(ReminderStatus.Pending);
        reminder.SentAt.ShouldBeNull();
        reminder.DeliveryLogs.Count.ShouldBe(1);                 // the old send stays on record
    }
}
