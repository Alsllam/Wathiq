using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace Wathiq.Documents.Documents;

/// <summary>
/// The value object behind FR-DOC-003. A pair of dates is not two independent fields: the rule
/// "expiry is not before issue" only makes sense on the pair, so the pair is the type. It is
/// immutable and compared by value (ABP ValueObject), which is what makes it safe to share.
/// </summary>
public class ValidityPeriod : ValueObject
{
    public DateOnly? IssueDate { get; }
    public DateOnly? ExpiryDate { get; }

    // EF Core materialises owned types through this; nobody else can reach an unchecked instance.
    private ValidityPeriod()
    {
    }

    public ValidityPeriod(DateOnly? issueDate, DateOnly? expiryDate)
    {
        if (issueDate.HasValue && expiryDate.HasValue && expiryDate.Value < issueDate.Value)
        {
            // BusinessException (not ArgumentException): this is a user-facing rule and gets a localized message.
            throw new BusinessException(DocumentsErrorCodes.ExpiryBeforeIssue)
                .WithData("IssueDate", issueDate.Value)
                .WithData("ExpiryDate", expiryDate.Value);
        }

        IssueDate = issueDate;
        ExpiryDate = expiryDate;
    }

    public static ValidityPeriod None => new(null, null);

    public bool IsExpiredOn(DateOnly today) => ExpiryDate.HasValue && ExpiryDate.Value < today;

    /// <summary>Days until expiry (negative when already expired); null when no expiry is known.</summary>
    public int? DaysUntilExpiry(DateOnly today) => ExpiryDate.HasValue ? ExpiryDate.Value.DayNumber - today.DayNumber : null;

    // Two periods with the same dates are the same value - there is no identity.
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return IssueDate;
        yield return ExpiryDate;
    }

    // ABP's ValueObject offers ValueEquals() but leaves Equals/GetHashCode alone; without these
    // overrides, ==, Shouldly and dictionary keys would still compare references.
    public override bool Equals(object? obj) => obj is ValidityPeriod other && ValueEquals(other);

    public override int GetHashCode() => HashCode.Combine(IssueDate, ExpiryDate);
}
