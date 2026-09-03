using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace Wathiq.Guides.Guides;

/// <summary>
/// Cross-aggregate rules (like HolderManager): slug uniqueness, next VersionNo, and the publish
/// handshake that touches BOTH aggregates (version becomes immutable, guide re-points) in one UoW.
/// </summary>
public class GuideManager : DomainService
{
    private readonly IRepository<Guide, Guid> _guides;
    private readonly IRepository<GuideVersion, Guid> _versions;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public GuideManager(
        IRepository<Guide, Guid> guides,
        IRepository<GuideVersion, Guid> versions,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _guides = guides;
        _versions = versions;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    public async Task<Guide> CreateAsync(string slug, string titleAr, string titleEn)
    {
        if (await _guides.AnyAsync(g => g.Slug == slug))
        {
            throw new BusinessException(GuidesErrorCodes.SlugAlreadyExists).WithData("Slug", slug);
        }

        // autoSave: callers in the SAME UoW (the seed: create → draft → publish) immediately
        // query this row back by id - without a flush the query hits the DB and finds nothing.
        return await _guides.InsertAsync(new Guide(_guidGenerator.Create(), slug, titleAr, titleEn), autoSave: true);
    }

    public async Task<GuideVersion> CreateDraftAsync(
        Guid guideId, string language, string bodyMarkdown, DateOnly lastVerifiedAt,
        string? requiredDocuments, string? fees, string? location, string[] steps)
    {
        await _guides.GetAsync(guideId);   // 404 semantics for a bad guide id

        // Next number across ALL languages of the guide: VersionNo is a single authoring
        // timeline (v1 ar, v2 en, v3 ar-revised...), unique per guide by index.
        var existing = await _versions.GetListAsync(v => v.GuideId == guideId);
        var nextNo = existing.Count == 0 ? 1 : existing.Max(v => v.VersionNo) + 1;

        var version = new GuideVersion(
            _guidGenerator.Create(), guideId, nextNo, language, bodyMarkdown,
            lastVerifiedAt, requiredDocuments, fees, location);
        version.ReplaceSteps(steps, _guidGenerator);

        return await _versions.InsertAsync(version, autoSave: true);   // same reason: PublishAsync re-reads by id
    }

    /// <summary>Publish = freeze the version AND make it the served one. Latest publish wins.</summary>
    public async Task<GuideVersion> PublishAsync(Guid versionId)
    {
        var version = await _versions.GetAsync(versionId);
        var guide = await _guides.GetAsync(version.GuideId);

        version.Publish(_clock.Now);
        guide.SetPublishedVersion(version);

        await _versions.UpdateAsync(version);
        await _guides.UpdateAsync(guide);
        return version;
    }
}
