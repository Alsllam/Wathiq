using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wathiq.Reminders.Reminders;

public interface IReminderAppService : IApplicationService
{
    /// <summary>Pending reminders, soonest first - the data behind the portal's "coming up" list.</summary>
    Task<PagedResultDto<ReminderDto>> GetUpcomingListAsync(PagedResultRequestDto input);
}
