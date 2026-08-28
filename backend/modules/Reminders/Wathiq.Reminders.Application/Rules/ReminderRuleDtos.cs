using System;
using System.ComponentModel.DataAnnotations;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.Rules;

/// <summary>The value object flattens to a plain int array on the wire (compare 1.6's date pair).</summary>
public class ReminderRuleDto
{
    public int[] OffsetsDays { get; set; } = [];
    public ReminderChannels Channels { get; set; }
    public TimeOnly? QuietFrom { get; set; }
    public TimeOnly? QuietTo { get; set; }
    public string TimeZoneId { get; set; } = default!;
}

public class UpdateReminderRuleDto
{
    [Required]
    [MinLength(1)]
    public int[] OffsetsDays { get; set; } = [];

    public ReminderChannels Channels { get; set; }

    public TimeOnly? QuietFrom { get; set; }
    public TimeOnly? QuietTo { get; set; }

    [Required]
    [StringLength(ReminderRuleConsts.MaxTimeZoneIdLength)]
    public string TimeZoneId { get; set; } = default!;
}
