using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wathiq.Guides.Guides;

/// <summary>Public reading side - no permission by design (5.1): guides are the community service.</summary>
public interface IGuideAppService : IApplicationService
{
    Task<ListResultDto<GuideDto>> GetListAsync();

    /// <summary>Served content by slug; <paramref name="language"/> picks ar/en (defaults to ar).</summary>
    Task<GuideDetailDto> GetBySlugAsync(string slug, string? language = null);
}
