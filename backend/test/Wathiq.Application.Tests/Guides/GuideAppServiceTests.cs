using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Wathiq.Guides.Data;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Guides;

/* FR-GDE-001/002 through the real stack: the seeded guide is publicly readable, authoring
 * creates and publishes new content, and the immutability rule survives DI + UoW + EF.
 * Concrete class lives in EntityFrameworkCore.Tests. */
public abstract class GuideAppServiceTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IGuideAppService _guides;
    private readonly IGuideAdminAppService _admin;

    protected GuideAppServiceTests()
    {
        _guides = GetRequiredService<IGuideAppService>();
        _admin = GetRequiredService<IGuideAdminAppService>();
    }

    [Fact]
    public async Task Seeded_Passport_Guide_Is_Served_In_Both_Languages()
    {
        var list = await _guides.GetListAsync();
        list.Items.ShouldContain(g => g.Slug == GuidesDataSeedContributor.PassportRenewalSlug);

        var ar = await _guides.GetBySlugAsync(GuidesDataSeedContributor.PassportRenewalSlug, "ar");
        ar.TitleAr.ShouldBe("تجديد جواز السفر");
        ar.Version.Language.ShouldBe("ar");
        ar.Version.PublishedAt.ShouldNotBeNull();
        ar.Version.Steps.Count.ShouldBe(6);
        ar.Version.LastVerifiedAt.ShouldBe(new DateOnly(2026, 9, 1));   // Vision R2 rides every read

        var en = await _guides.GetBySlugAsync(GuidesDataSeedContributor.PassportRenewalSlug, "en");
        en.Version.Language.ShouldBe("en");
        en.Version.BodyMarkdown.ShouldContain("Absher");
    }

    [Fact]
    public async Task Authoring_Flow_Creates_Publishes_And_Then_Freezes()
    {
        var guide = await _admin.CreateAsync(new CreateGuideDto
        {
            Slug = $"renew-licence-{Guid.NewGuid():N}"[..40],
            TitleAr = "تجديد رخصة القيادة",
            TitleEn = "Renew a driving licence"
        });

        var draft = await _admin.CreateVersionAsync(new CreateGuideVersionDto
        {
            GuideId = guide.Id,
            Language = "ar",
            BodyMarkdown = "## الرسوم\n40 ريالًا لكل سنة.",
            LastVerifiedAt = new DateOnly(2026, 9, 1),
            Steps = ["افحص المركبة", "سدّد الرسوم", "جدّد عبر أبشر"]
        });
        draft.VersionNo.ShouldBe(1);
        draft.PublishedAt.ShouldBeNull();

        // Draft is invisible to the public list until published.
        (await _guides.GetListAsync()).Items.ShouldNotContain(g => g.Id == guide.Id);

        // Drafts stay editable...
        var edited = await _admin.UpdateVersionAsync(draft.Id, new UpdateGuideVersionDto
        {
            BodyMarkdown = "## الرسوم\n40 ريالًا لكل سنة من مدة الرخصة.",
            LastVerifiedAt = new DateOnly(2026, 9, 2),
            Steps = ["افحص المركبة", "سدّد الرسوم", "جدّد عبر أبشر", "استلم الرخصة"]
        });
        edited.Steps.Count.ShouldBe(4);

        var published = await _admin.PublishAsync(draft.Id);
        published.PublishedAt.ShouldNotBeNull();

        // ...and published versions are frozen, through the full service stack.
        var frozen = await Should.ThrowAsync<BusinessException>(() =>
            _admin.UpdateVersionAsync(draft.Id, new UpdateGuideVersionDto
            {
                BodyMarkdown = "tamper",
                LastVerifiedAt = new DateOnly(2026, 9, 3)
            }));
        frozen.Code.ShouldBe(GuidesErrorCodes.PublishedVersionIsImmutable);

        // The public read now serves it, steps in order.
        var detail = await _guides.GetBySlugAsync(guide.Slug, "ar");
        detail.Version.Id.ShouldBe(draft.Id);
        detail.Version.Steps.Last().ShouldBe("استلم الرخصة");
    }

    [Fact]
    public async Task Duplicate_Slug_Is_Rejected()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() => _admin.CreateAsync(new CreateGuideDto
        {
            Slug = GuidesDataSeedContributor.PassportRenewalSlug,
            TitleAr = "نسخة",
            TitleEn = "Duplicate"
        }));
        ex.Code.ShouldBe(GuidesErrorCodes.SlugAlreadyExists);
    }
}
