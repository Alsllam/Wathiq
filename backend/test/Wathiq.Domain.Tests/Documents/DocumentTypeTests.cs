using System;
using Shouldly;
using Wathiq.Documents.DocumentTypes;
using Xunit;

namespace Wathiq.Documents;

// Pure domain rules need no ABP host: plain xUnit, runs in Wathiq.Domain.Tests directly.
public class DocumentTypeTests
{
    [Fact]
    public void Should_Normalise_Code_And_Start_Active()
    {
        var type = new DocumentType(Guid.NewGuid(), "passport", "جواز السفر", "Passport", 120);

        type.Code.ShouldBe("PASSPORT");
        type.IsActive.ShouldBeTrue();
        type.DefaultValidityMonths.ShouldBe(120);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_Blank_Code(string code)
    {
        Should.Throw<ArgumentException>(() => new DocumentType(Guid.NewGuid(), code, "أ", "A"));
    }

    [Fact]
    public void Should_Reject_Non_Positive_Validity()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new DocumentType(Guid.NewGuid(), "X", "أ", "A", 0));
    }
}
