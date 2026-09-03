using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Wathiq.Guides.Guides;

/// <summary>Authoring side, admin-only (Guides.Manage). Drafts are editable; publish freezes.</summary>
public interface IGuideAdminAppService : IApplicationService
{
    Task<GuideDto> CreateAsync(CreateGuideDto input);
    Task<GuideVersionDto> CreateVersionAsync(CreateGuideVersionDto input);
    Task<GuideVersionDto> UpdateVersionAsync(Guid id, UpdateGuideVersionDto input);
    Task<GuideVersionDto> PublishAsync(Guid id);

    /// <summary>Re-enqueue chunking+embedding for a published version (seeded content, model swaps).</summary>
    Task RebuildEmbeddingsAsync(Guid id);
}
