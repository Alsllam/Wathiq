using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wathiq.Guides.Feedback;

/// <summary>FR-GDE feedback loop: anyone reports, the admin reads and resolves.</summary>
public interface IGuideFeedbackAppService : IApplicationService
{
    Task CreateAsync(CreateGuideFeedbackDto input);
    Task<ListResultDto<GuideFeedbackDto>> GetListAsync(bool includeResolved = false);
    Task ResolveAsync(Guid id);
}
