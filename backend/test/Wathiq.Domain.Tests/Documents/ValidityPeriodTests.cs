using System;
using Shouldly;
using Volo.Abp;
using Wathiq.Documents.Documents;
using Xunit;

namespace Wathiq.Documents;

public class ValidityPeriodTests
{
    private static readonly DateOnly Issue = new(2026, 1, 15);

    [Fact]
    public void Should_Reject_Expiry_Before_Issue()
    {
        var ex = Should.Throw<BusinessException>(() => new ValidityPeriod(Issue, Issue.AddDays(-1)));
        ex.Code.ShouldBe(DocumentsErrorCodes.ExpiryBeforeIssue);
    }

    [Fact]
    public void Same_Day_Issue_And_Expiry_Is_Allowed()
    {
        new ValidityPeriod(Issue, Issue).ExpiryDate.ShouldBe(Issue);
    }

    [Fact]
    public void Missing_Dates_Skip_The_Rule()
    {
        new ValidityPeriod(null, Issue).ExpiryDate.ShouldBe(Issue);
        new ValidityPeriod(Issue, null).ExpiryDate.ShouldBeNull();
        ValidityPeriod.None.DaysUntilExpiry(Issue).ShouldBeNull();
    }

    [Fact]
    public void Is_Compared_By_Value()
    {
        var a = new ValidityPeriod(Issue, Issue.AddYears(5));
        var b = new ValidityPeriod(Issue, Issue.AddYears(5));

        a.ShouldBe(b);                 // ValueObject.Equals
        a.ValueEquals(b).ShouldBeTrue();
        ReferenceEquals(a, b).ShouldBeFalse();
    }

    [Fact]
    public void Computes_Expiry_Math()
    {
        var period = new ValidityPeriod(Issue, new DateOnly(2026, 3, 1));

        period.DaysUntilExpiry(new DateOnly(2026, 2, 1)).ShouldBe(28);
        period.IsExpiredOn(new DateOnly(2026, 3, 1)).ShouldBeFalse(); // expires end of that day
        period.IsExpiredOn(new DateOnly(2026, 3, 2)).ShouldBeTrue();
    }
}
