using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Wathiq.Guides.Guides;

/// <summary>
/// Authoring endpoints (/api/guides/guide-admin/*). One [Authorize] at class level: every
/// method is admin work; the domain (GuideVersion.EnsureDraft) enforces immutability
/// regardless of who calls.
/// </summary>
[Authorize(GuidesPermissions.Guides.Manage)]
public class GuideAdminAppService : GuidesAppServiceBase, IGuideAdminAppService
{
    private readonly GuideManager _guideManager;
    private readonly IRepository<GuideVersion, Guid> _versions;

    public GuideAdminAppService(GuideManager guideManager, IRepository<GuideVersion, Guid> versions)
    {
        _guideManager = guideManager;
        _versions = versions;
    }

    public async Task<GuideDto> CreateAsync(CreateGuideDto input)
    {
        var guide = await _guideManager.CreateAsync(input.Slug, input.TitleAr, input.TitleEn);
        return new GuideDto { Id = guide.Id, Slug = guide.Slug, TitleAr = guide.TitleAr, TitleEn = guide.TitleEn };
    }

    public async Task<GuideVersionDto> CreateVersionAsync(CreateGuideVersionDto input)
    {
        var version = await _guideManager.CreateDraftAsync(
            input.GuideId, input.Language, input.BodyMarkdown, input.LastVerifiedAt,
            input.RequiredDocuments, input.Fees, input.Location, input.Steps);
        return ToVersionDto(version);
    }

    public async Task<GuideVersionDto> UpdateVersionAsync(Guid id, UpdateGuideVersionDto input)
    {
        var version = await _versions.GetAsync(id);

        // Domain guards immutability (PublishedVersionIsImmutable) - the service just relays.
        version.UpdateDraft(input.BodyMarkdown, input.LastVerifiedAt, input.RequiredDocuments, input.Fees, input.Location);
        version.ReplaceSteps(input.Steps, GuidGenerator);

        await _versions.UpdateAsync(version);
        return ToVersionDto(version);
    }

    public async Task<GuideVersionDto> PublishAsync(Guid id)
    {
        return ToVersionDto(await _guideManager.PublishAsync(id));
    }

    internal static GuideVersionDto ToVersionDto(GuideVersion v) => new()
    {
        Id = v.Id,
        GuideId = v.GuideId,
        VersionNo = v.VersionNo,
        Language = v.Language,
        BodyMarkdown = v.BodyMarkdown,
        RequiredDocuments = v.RequiredDocuments,
        Fees = v.Fees,
        Location = v.Location,
        LastVerifiedAt = v.LastVerifiedAt,
        PublishedAt = v.PublishedAt,
        Steps = v.Steps.OrderBy(s => s.StepNo).Select(s => s.Text).ToList()
    };
}
