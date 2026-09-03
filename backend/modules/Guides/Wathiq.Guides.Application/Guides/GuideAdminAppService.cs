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
    private readonly Volo.Abp.BackgroundJobs.IBackgroundJobManager _backgroundJobManager;

    public GuideAdminAppService(
        GuideManager guideManager,
        IRepository<GuideVersion, Guid> versions,
        Volo.Abp.BackgroundJobs.IBackgroundJobManager backgroundJobManager)
    {
        _guideManager = guideManager;
        _versions = versions;
        _backgroundJobManager = backgroundJobManager;
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

    public async Task RebuildEmbeddingsAsync(Guid id)
    {
        var version = await _versions.GetAsync(id);
        if (!version.IsPublished)
        {
            // Only published (immutable) content is embeddable - a draft's vectors would go
            // stale on the next edit and 5.5 must never cite a draft.
            throw new Volo.Abp.BusinessException(GuidesErrorCodes.VersionNotPublished);
        }

        // Publish already embeds via the event; this is the manual lever for versions published
        // BEFORE the pipeline existed (the seed) and for re-embedding after a model change.
        await _backgroundJobManager.EnqueueAsync(new Embedding.GuideEmbedArgs { GuideVersionId = version.Id });
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
