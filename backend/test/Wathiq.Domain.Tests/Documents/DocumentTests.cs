using System;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Wathiq.Documents.Documents;
using Xunit;

namespace Wathiq.Documents;

public class DocumentTests
{
    private static Document NewDocument() => new(
        Guid.NewGuid(), ownerUserId: Guid.NewGuid(), holderId: Guid.NewGuid(), documentTypeId: Guid.NewGuid(),
        new ValidityPeriod(new DateOnly(2021, 1, 1), new DateOnly(2026, 1, 1)), number: "A123");

    [Fact]
    public void MarkRenewed_Keeps_Previous_Expiry_And_Reactivates()
    {
        var doc = NewDocument().Archive();

        doc.MarkRenewed(new ValidityPeriod(new DateOnly(2026, 1, 2), new DateOnly(2031, 1, 2)));

        doc.PreviousExpiryDate.ShouldBe(new DateOnly(2026, 1, 1));
        doc.Validity.ExpiryDate.ShouldBe(new DateOnly(2031, 1, 2));
        doc.Status.ShouldBe(DocumentStatus.Active);
    }

    [Fact]
    public void Attachments_Are_Managed_Only_Through_The_Root()
    {
        var doc = NewDocument();
        var id = Guid.NewGuid();

        doc.AddAttachment(id, "blob-1.jpg", "image/jpeg", 1234, new byte[32]);
        doc.Attachments.Count.ShouldBe(1);
        doc.Attachments.ShouldAllBe(a => a.DocumentId == doc.Id);

        doc.RemoveAttachment(id).ShouldBe("blob-1.jpg");
        doc.Attachments.ShouldBeEmpty();
        Should.Throw<EntityNotFoundException>(() => doc.RemoveAttachment(id));
    }

    [Fact]
    public void Blank_Number_Becomes_Null()
    {
        NewDocument().SetNumber("   ").Number.ShouldBeNull();
    }
}
