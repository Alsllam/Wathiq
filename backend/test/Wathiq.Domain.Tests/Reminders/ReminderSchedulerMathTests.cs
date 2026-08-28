using System;
using System.Linq;
using Shouldly;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;
using Xunit;

namespace Wathiq.Reminders;

/* The scheduling math is static and clock-free - "testing time" means passing dates, not mocking clocks. */
public class ReminderSchedulerMathTests
{
    private static readonly DateOnly Today = new(2026, 8, 27);

    [Fact]
    public void Full_Schedule_When_Expiry_Is_Far_Away()
    {
        var schedule = ReminderScheduler.ComputeSchedule(ReminderOffsets.Default, Today.AddYears(1), Today);

        schedule.Select(x => x.OffsetDays).ShouldBe([90, 30, 7, 1]);
        schedule.First().DueDate.ShouldBe(Today.AddYears(1).AddDays(-90));
    }

    [Fact]
    public void Past_Due_Dates_Are_Skipped_But_Today_Still_Counts()
    {
        // Expiry in 30 days: the 90-day reminder is 60 days in the past, the 30-day one is due TODAY.
        var schedule = ReminderScheduler.ComputeSchedule(ReminderOffsets.Default, Today.AddDays(30), Today);

        schedule.Select(x => x.OffsetDays).ShouldBe([30, 7, 1]);
        schedule.First().DueDate.ShouldBe(Today);
    }

    [Fact]
    public void No_Expiry_Means_No_Reminders()
    {
        ReminderScheduler.ComputeSchedule(ReminderOffsets.Default, null, Today).ShouldBeEmpty();
    }

    [Fact]
    public void Year_Boundary_Subtraction_Is_Calendar_Correct()
    {
        // 90 days before 2027-02-15 crosses two month lengths and a year boundary.
        var schedule = ReminderScheduler.ComputeSchedule(new ReminderOffsets([90]), new DateOnly(2027, 2, 15), Today);

        schedule.Single().DueDate.ShouldBe(new DateOnly(2026, 11, 17));
    }

    [Fact]
    public void Today_Depends_On_The_Users_Time_Zone()
    {
        // 21:30 UTC: still the 27th in London, already the 28th in Riyadh (UTC+3).
        var utc = new DateTime(2026, 8, 27, 21, 30, 0, DateTimeKind.Utc);

        ReminderScheduler.TodayIn("Etc/UTC", utc).ShouldBe(new DateOnly(2026, 8, 27));
        ReminderScheduler.TodayIn("Asia/Riyadh", utc).ShouldBe(new DateOnly(2026, 8, 28));
        // And the other direction: 01:30 UTC is still "yesterday" in Los Angeles.
        ReminderScheduler.TodayIn("America/Los_Angeles", new DateTime(2026, 8, 27, 1, 30, 0, DateTimeKind.Utc))
            .ShouldBe(new DateOnly(2026, 8, 26));
    }
}
