using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Wathiq.Guides.Guides;

/// <summary>
/// The public reading surface. [AllowAnonymous] is the 5.1 permission design made real:
/// published guides are served to anyone, signed in or not.
/// </summary>
[AllowAnonymous]
public class GuideAppService : GuidesAppServiceBase, IGuideAppService
{
    private readonly IRepository<Guide, Guid> _guides;
    private readonly IRepository<GuideVersion, Guid> _versions;

    public GuideAppService(IRepository<Guide, Guid> guides, IRepository<GuideVersion, Guid> versions)
    {
        _guides = guides;
        _versions = versions;
    }

    public async Task<ListResultDto<GuideDto>> GetListAsync()
    {
        var query = (await _guides.GetQueryableAsync())
            // Only guides with something published: a catalogue row you cannot open is a broken promise.
            .Where(g => g.IsActive && g.PublishedVersionId != null)
            .OrderBy(g => g.Slug);

        var guides = await AsyncExecuter.ToListAsync(query);

        return new ListResultDto<GuideDto>(guides.Select(ToListDto).ToList());
    }

    public async Task<GuideDetailDto> GetBySlugAsync(string slug, string? language = null)
    {
        var guide = await _guides.GetAsync(g => g.Slug == slug && g.IsActive);

        // Per-language serving: latest PUBLISHED version in the requested language (ar default,
        // Arabic-first), falling back to whatever Guide.PublishedVersionId points at.
        var lang = GuideConsts.SupportedLanguages.Contains(language) ? language! : "ar";

        var versions = await _versions.GetListAsync(v => v.GuideId == guide.Id && v.PublishedAt != null);
        var version =
            versions.Where(v => v.Language == lang).OrderByDescending(v => v.VersionNo).FirstOrDefault()
            ?? versions.FirstOrDefault(v => v.Id == guide.PublishedVersionId);

        if (version is null)
        {
            throw new BusinessException(GuidesErrorCodes.GuideNotPublished);
        }

        return new GuideDetailDto
        {
            Id = guide.Id,
            Slug = guide.Slug,
            TitleAr = guide.TitleAr,
            TitleEn = guide.TitleEn,
            Version = GuideAdminAppService.ToVersionDto(version)
        };
    }

    private static GuideDto ToListDto(Guide g) => new()
    {
        Id = g.Id,
        Slug = g.Slug,
        TitleAr = g.TitleAr,
        TitleEn = g.TitleEn
    };
}
