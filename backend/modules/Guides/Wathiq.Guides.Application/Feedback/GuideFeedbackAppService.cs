using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.Feedback;

/// <summary>
/// The asymmetry a third time: CREATING feedback is anonymous (readers are anonymous - an
/// "outdated" report gated behind sign-in is a report never filed), reading and resolving it
/// is admin work. Method-level attributes override the class default here.
/// </summary>
public class GuideFeedbackAppService : GuidesAppServiceBase, IGuideFeedbackAppService
{
    private readonly IRepository<GuideFeedback, Guid> _feedback;
    private readonly IRepository<GuideVersion, Guid> _versions;
    private readonly IRepository<Guide, Guid> _guides;

    public GuideFeedbackAppService(
        IRepository<GuideFeedback, Guid> feedback,
        IRepository<GuideVersion, Guid> versions,
        IRepository<Guide, Guid> guides)
    {
        _feedback = feedback;
        _versions = versions;
        _guides = guides;
    }

    [AllowAnonymous]
    public async Task CreateAsync(CreateGuideFeedbackDto input)
    {
        var version = await _versions.GetAsync(input.GuideVersionId);
        if (!version.IsPublished)
        {
            // Readers only ever see published versions; feedback on a draft means a forged id.
            throw new BusinessException(GuidesErrorCodes.VersionNotPublished);
        }

        await _feedback.InsertAsync(new GuideFeedback(
            GuidGenerator.Create(), version.Id, CurrentUser.Id, input.Kind, input.Comment));
    }

    [Authorize(GuidesPermissions.Guides.Manage)]
    public async Task<ListResultDto<GuideFeedbackDto>> GetListAsync(bool includeResolved = false)
    {
        var items = await _feedback.GetListAsync(f => includeResolved || f.ResolvedAt == null);

        // Slugs give the admin context without a second request; corpus-sized joins in memory.
        var versionIds = items.Select(f => f.GuideVersionId).Distinct().ToList();
        var versions = (await _versions.GetListAsync(v => versionIds.Contains(v.Id))).ToDictionary(v => v.Id);
        var guideIds = versions.Values.Select(v => v.GuideId).Distinct().ToList();
        var guides = (await _guides.GetListAsync(g => guideIds.Contains(g.Id))).ToDictionary(g => g.Id);

        return new ListResultDto<GuideFeedbackDto>(items
            .OrderByDescending(f => f.CreationTime)
            .Select(f => new GuideFeedbackDto
            {
                Id = f.Id,
                GuideVersionId = f.GuideVersionId,
                GuideSlug = guides[versions[f.GuideVersionId].GuideId].Slug,
                Kind = f.Kind,
                Comment = f.Comment,
                Anonymous = f.UserId is null,   // WHO is admin-irrelevant; WHETHER identified is not
                CreationTime = f.CreationTime,
                ResolvedAt = f.ResolvedAt
            }).ToList());
    }

    [Authorize(GuidesPermissions.Guides.Manage)]
    public async Task ResolveAsync(Guid id)
    {
        var feedback = await _feedback.GetAsync(id);
        feedback.Resolve(Clock.Now);
        await _feedback.UpdateAsync(feedback);
    }
}
