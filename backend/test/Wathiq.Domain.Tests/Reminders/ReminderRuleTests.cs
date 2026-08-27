using System;
using Shouldly;
using Volo.Abp;
using Wathiq.Reminders.Rules;
using Xunit;

namespace Wathiq.Reminders;

public class ReminderRuleTests
{
    private static ReminderRule NewRule() => new(
        Guid.NewGuid(), Guid.NewGuid(), ReminderOffsets.Default, ReminderChannels.Email,
        ReminderRuleConsts.DefaultTimeZoneId);

    [Fact]
    public void Quiet_Hours_Need_Both_Bounds_Or_Neither()
    {
        var rule = NewRule();

        Should.Throw<BusinessException>(() => rule.SetQuietHours(new TimeOnly(22, 0), null))
            .Code.ShouldBe(RemindersErrorCodes.QuietHoursIncomplete);

        // Over midnight is a valid window, not an error (22:00 -> 07:00).
        rule.SetQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));
        rule.QuietFrom.ShouldBe(new TimeOnly(22, 0));

        rule.SetQuietHours(null, null);                 // clearing is allowed
        rule.QuietFrom.ShouldBeNull();
    }

    [Fact]
    public void Time_Zone_Must_Be_A_Real_Iana_Id()
    {
        var rule = NewRule();

        rule.SetTimeZone("Europe/Berlin").TimeZoneId.ShouldBe("Europe/Berlin");

        Should.Throw<BusinessException>(() => rule.SetTimeZone("Mars/Olympus"))
            .Code.ShouldBe(RemindersErrorCodes.UnknownTimeZone);
        rule.TimeZoneId.ShouldBe("Europe/Berlin");      // failed set leaves the rule untouched
    }

    [Fact]
    public void Channels_None_Pauses_Without_Losing_Offsets()
    {
        var rule = NewRule().SetChannels(ReminderChannels.None);

        rule.Channels.ShouldBe(ReminderChannels.None);
        rule.Offsets.ShouldBe(ReminderOffsets.Default);

        rule.SetChannels(ReminderChannels.Email | ReminderChannels.Push);
        rule.Channels.HasFlag(ReminderChannels.Push).ShouldBeTrue();
    }
}
