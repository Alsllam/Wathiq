using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Wathiq.Guides.Feedback;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Guides;

/* The feedback loop end to end: an anonymous reader flags a published version, the admin sees
 * and resolves it, and forged/draft version ids are rejected. Concrete class in EFCore.Tests. */
public abstract class GuideFeedbackAppServiceTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IGuideFeedbackAppService _feedback;
    private readonly IGuideAppService _guides;
    private readonly IGuideAdminAppService _admin;

    protected GuideFeedbackAppServiceTests()
    {
        _feedback = GetRequiredService<IGuideFeedbackAppService>();
        _guides = GetRequiredService<IGuideAppService>();
        _admin = GetRequiredService<IGuideAdminAppService>();
    }

    [Fact]
    public async Task Anonymous_Flag_Reaches_The_Admin_And_Gets_Resolved()
    {
        // The version id a real reader would have: from the public read model.
        var detail = await _guides.GetBySlugAsync("renew-passport", "ar");

        await _feedback.CreateAsync(new CreateGuideFeedbackDto
        {
            GuideVersionId = detail.Version.Id,
            Kind = GuideFeedbackKind.Outdated,
            Comment = "الرسوم تغيّرت هذا الشهر"
        });

        var open = (await _feedback.GetListAsync()).Items
            .Where(f => f.GuideVersionId == detail.Version.Id && f.Comment == "الرسوم تغيّرت هذا الشهر")
            .ToList();
        var item = open.ShouldHaveSingleItem();
        item.GuideSlug.ShouldBe("renew-passport");   // the admin sees context, not bare Guids
        item.ResolvedAt.ShouldBeNull();

        await _feedback.ResolveAsync(item.Id);
        await _feedback.ResolveAsync(item.Id);   // resolving twice is a no-op, not an error

        (await _feedback.GetListAsync()).Items.ShouldNotContain(f => f.Id == item.Id);   // default list = open only
        (await _feedback.GetListAsync(includeResolved: true)).Items
            .Single(f => f.Id == item.Id).ResolvedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task A_Draft_Version_Cannot_Receive_Feedback()
    {
        var guide = await _admin.CreateAsync(new CreateGuideDto
        {
            Slug = $"fb-{Guid.NewGuid():N}"[..20],
            TitleAr = "مسودة",
            TitleEn = "Draft"
        });
        var draft = await _admin.CreateVersionAsync(new CreateGuideVersionDto
        {
            GuideId = guide.Id,
            Language = "ar",
            BodyMarkdown = "مسودة",
            LastVerifiedAt = new DateOnly(2026, 9, 1)
        });

        // Readers never see drafts, so a draft id in feedback is a forged request.
        (await Should.ThrowAsync<BusinessException>(() => _feedback.CreateAsync(new CreateGuideFeedbackDto
        {
            GuideVersionId = draft.Id,
            Kind = GuideFeedbackKind.Wrong
        }))).Code.ShouldBe(GuidesErrorCodes.VersionNotPublished);
    }
}
