using System;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Guids;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Guides;

/* The module's one hard rule, tested where it lives: a published GuideVersion refuses every
 * mutation. 5.3's chunks and 5.5's citations will point at these ids - immutability here is
 * what makes those references stay true. */
public class GuidePublishWorkflowTests
{
    private static readonly IGuidGenerator Guids = SimpleGuidGenerator.Instance;

    private static GuideVersion NewDraft(Guid? guideId = null) => new(
        Guids.Create(), guideId ?? Guids.Create(), versionNo: 1, language: "ar",
        bodyMarkdown: "## تجديد", lastVerifiedAt: new DateOnly(2026, 9, 1));

    [Fact]
    public void Publish_Sets_PublishedAt_Once_And_Only_Once()
    {
        var version = NewDraft();
        version.IsPublished.ShouldBeFalse();

        version.Publish(new DateTime(2026, 9, 3, 12, 0, 0, DateTimeKind.Utc));

        version.IsPublished.ShouldBeTrue();
        Should.Throw<BusinessException>(() => version.Publish(DateTime.UtcNow))
            .Code.ShouldBe(GuidesErrorCodes.VersionAlreadyPublished);
    }

    [Fact]
    public void Published_Version_Refuses_Every_Edit()
    {
        var version = NewDraft();
        version.ReplaceSteps(["الخطوة الأولى"], Guids);
        version.Publish(DateTime.UtcNow);

        Should.Throw<BusinessException>(() =>
                version.UpdateDraft("edited", new DateOnly(2026, 9, 2), null, null, null))
            .Code.ShouldBe(GuidesErrorCodes.PublishedVersionIsImmutable);

        Should.Throw<BusinessException>(() => version.ReplaceSteps(["غش"], Guids))
            .Code.ShouldBe(GuidesErrorCodes.PublishedVersionIsImmutable);

        // Nothing changed: the frozen content is exactly what was published.
        version.BodyMarkdown.ShouldBe("## تجديد");
        version.Steps.ShouldHaveSingleItem().Text.ShouldBe("الخطوة الأولى");
    }

    [Fact]
    public void ReplaceSteps_Numbers_From_One_In_Order()
    {
        var version = NewDraft();

        version.ReplaceSteps(["سدّد الرسوم", "ادخل أبشر", "أرسل الطلب"], Guids);

        version.Steps.Count.ShouldBe(3);
        version.Steps[0].StepNo.ShouldBe(1);
        version.Steps[2].StepNo.ShouldBe(3);
        version.Steps[2].Text.ShouldBe("أرسل الطلب");
        version.Steps.ShouldAllBe(s => s.GuideVersionId == version.Id);
    }

    [Fact]
    public void Guide_Serves_Only_Its_Own_Published_Versions()
    {
        var guide = new Guide(Guids.Create(), "renew-passport", "تجديد جواز السفر", "Renew a passport");

        var draft = NewDraft(guide.Id);
        Should.Throw<BusinessException>(() => guide.SetPublishedVersion(draft))
            .Code.ShouldBe(GuidesErrorCodes.CannotServeDraft);

        var foreign = NewDraft();   // belongs to a different guide
        foreign.Publish(DateTime.UtcNow);
        Should.Throw<BusinessException>(() => guide.SetPublishedVersion(foreign))
            .Code.ShouldBe(GuidesErrorCodes.VersionNotOfGuide);

        draft.Publish(DateTime.UtcNow);
        guide.SetPublishedVersion(draft);
        guide.PublishedVersionId.ShouldBe(draft.Id);
    }

    [Fact]
    public void Language_Is_Restricted_To_Supported_Set()
    {
        Should.Throw<ArgumentException>(() => new GuideVersion(
            Guids.Create(), Guids.Create(), 1, "fr", "body", new DateOnly(2026, 9, 1)));
    }
}
