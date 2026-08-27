using Shouldly;
using Volo.Abp;
using Wathiq.Reminders.Rules;
using Xunit;

namespace Wathiq.Reminders;

public class ReminderOffsetsTests
{
    [Fact]
    public void Default_Matches_The_SRS()
    {
        ReminderOffsets.Default.Days.ShouldBe([90, 30, 7, 1]);
    }

    [Fact]
    public void Normalizes_Order_And_Duplicates_And_Round_Trips_As_Csv()
    {
        var offsets = new ReminderOffsets([7, 90, 7, 1, 30]);

        offsets.Days.ShouldBe([90, 30, 7, 1]);          // descending, distinct
        offsets.ToCsv().ShouldBe("90,30,7,1");
        ReminderOffsets.FromCsv(" 90, 30 ,7,1 ").ShouldBe(offsets); // tolerant parse, value equality
    }

    [Theory]
    [InlineData(new int[0])]
    [InlineData(new[] { 0 })]
    [InlineData(new[] { -5, 30 })]
    [InlineData(new[] { 4000 })]
    [InlineData(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]   // more than MaxOffsetCount
    public void Rejects_Invalid_Sets(int[] days)
    {
        var ex = Should.Throw<BusinessException>(() => new ReminderOffsets(days));
        ex.Code.ShouldBe(RemindersErrorCodes.InvalidOffsets);
    }
}
