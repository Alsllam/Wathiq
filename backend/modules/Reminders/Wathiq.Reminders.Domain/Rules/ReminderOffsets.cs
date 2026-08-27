using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace Wathiq.Reminders.Rules;

/// <summary>
/// "How many days before expiry to remind" as one value (FR-REM-001). The set is the unit —
/// ordering, de-duplication and bounds are rules of the whole list, so the list is the type.
/// Persisted as a CSV string via an EF value conversion (one column), unlike ValidityPeriod
/// which is an owned type (two columns): conversion for one scalar, ownership for a struct-of-columns.
/// </summary>
public class ReminderOffsets : ValueObject
{
    public IReadOnlyList<int> Days { get; } = default!;

    private ReminderOffsets()
    {
    }

    public ReminderOffsets(IEnumerable<int> days)
    {
        var normalized = days?.Distinct().OrderByDescending(d => d).ToArray() ?? [];

        if (normalized.Length == 0 || normalized.Length > ReminderRuleConsts.MaxOffsetCount
            || normalized.Any(d => d < 1 || d > ReminderRuleConsts.MaxOffsetDays))
        {
            // BusinessException: users edit these; the message must come back localized.
            throw new BusinessException(RemindersErrorCodes.InvalidOffsets)
                .WithData("Max", ReminderRuleConsts.MaxOffsetCount);
        }

        Days = normalized;
    }

    /// <summary>SRS default: 90, 30, 7 and 1 days before expiry.</summary>
    public static ReminderOffsets Default => new([90, 30, 7, 1]);

    public static ReminderOffsets FromCsv(string csv) =>
        new((csv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse));

    public string ToCsv() => string.Join(',', Days);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        foreach (var day in Days)
        {
            yield return day;
        }
    }

    // Same override pair as ValidityPeriod (1.5): ABP's ValueObject gives ValueEquals only.
    public override bool Equals(object? obj) => obj is ReminderOffsets other && ValueEquals(other);

    public override int GetHashCode() => Days.Aggregate(17, HashCode.Combine);
}
