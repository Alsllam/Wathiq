using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Reminders.Rules;

/// <summary>One per user (UQ UserId): when and through which channels they want reminding.</summary>
public class ReminderRule : FullAuditedAggregateRoot<Guid>
{
    public Guid UserId { get; private set; }   // plain Guid, no FK to Identity (DB2)
    public ReminderOffsets Offsets { get; private set; } = default!;
    public ReminderChannels Channels { get; private set; }
    public TimeOnly? QuietFrom { get; private set; }
    public TimeOnly? QuietTo { get; private set; }
    public string TimeZoneId { get; private set; } = default!;

    private ReminderRule()
    {
    }

    public ReminderRule(Guid id, Guid userId, ReminderOffsets offsets, ReminderChannels channels, string timeZoneId)
        : base(id)
    {
        UserId = userId;
        SetOffsets(offsets);
        SetChannels(channels);
        SetTimeZone(timeZoneId);
    }

    public ReminderRule SetOffsets(ReminderOffsets offsets)
    {
        Offsets = Check.NotNull(offsets, nameof(offsets));
        return this;
    }

    public ReminderRule SetChannels(ReminderChannels channels)
    {
        // None is allowed: "pause reminders" without deleting the rule (offsets survive).
        Channels = channels;
        return this;
    }

    /// <summary>Both bounds or neither: a half-open quiet window has no meaning (FR-REM-003).</summary>
    public ReminderRule SetQuietHours(TimeOnly? from, TimeOnly? to)
    {
        if (from.HasValue != to.HasValue)
        {
            throw new BusinessException(RemindersErrorCodes.QuietHoursIncomplete);
        }

        // from > to is legal and means "over midnight" (e.g. 22:00 -> 07:00) - no check here.
        QuietFrom = from;
        QuietTo = to;
        return this;
    }

    public ReminderRule SetTimeZone(string timeZoneId)
    {
        Check.NotNullOrWhiteSpace(timeZoneId, nameof(timeZoneId), ReminderRuleConsts.MaxTimeZoneIdLength);

        // .NET resolves IANA ids via ICU on every OS - validate now so the nightly job never has to.
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
        {
            throw new BusinessException(RemindersErrorCodes.UnknownTimeZone).WithData("TimeZoneId", timeZoneId);
        }

        TimeZoneId = timeZoneId;
        return this;
    }
}
