using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Wathiq.Guides.Chat;

/// <summary>FR-GDE-004: grounded Q&amp;A over the published guides (/api/guides/chat/*).</summary>
public interface IChatAppService : IApplicationService
{
    Task<GuideChatResponseDto> AskAsync(GuideChatRequestDto input);
}
