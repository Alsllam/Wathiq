using System;
using Shouldly;
using Wathiq.Documents.Holders;
using Xunit;

namespace Wathiq.Documents;

public class HolderTests
{
    [Fact]
    public void IsSelf_Is_Derived_From_Relation()
    {
        var userId = Guid.NewGuid();

        new Holder(Guid.NewGuid(), userId, "Amina", HolderRelation.Self).IsSelf.ShouldBeTrue();
        new Holder(Guid.NewGuid(), userId, "Sara", HolderRelation.Child).IsSelf.ShouldBeFalse();
    }

    [Fact]
    public void Should_Reject_Future_Birth_Date()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        Should.Throw<ArgumentOutOfRangeException>(() =>
            new Holder(Guid.NewGuid(), Guid.NewGuid(), "Sara", HolderRelation.Child, tomorrow));
    }
}
