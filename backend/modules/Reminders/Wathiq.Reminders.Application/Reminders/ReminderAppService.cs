using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Wathiq.Reminders.Reminders;

[Authorize(RemindersPermissions.Reminders.Default)]
public class ReminderAppService : RemindersAppServiceBase, IReminderAppService
{
    private readonly IRepository<Reminder, Guid> _reminders;

    public ReminderAppService(IRepository<Reminder, Guid> reminders)
    {
        _reminders = reminders;
    }

    public async Task<PagedResultDto<ReminderDto>> GetUpcomingListAsync(PagedResultRequestDto input)
    {
        var query = (await _reminders.GetQueryableAsync())
            .Where(r => r.UserId == CurrentUser.GetId() && r.Status == ReminderStatus.Pending);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var page = await AsyncExecuter.ToListAsync(query
            .OrderBy(r => r.DueDate).ThenBy(r => r.OffsetDays)
            .PageBy(input));

        return new PagedResultDto<ReminderDto>(totalCount, page.Select(ToDto).ToList());
    }

    private static ReminderDto ToDto(Reminder r) => new()
    {
        Id = r.Id,
        DocumentId = r.DocumentId,
        OffsetDays = r.OffsetDays,
        DueDate = r.DueDate,
        ExpiryDate = r.DueDate.AddDays(r.OffsetDays),
        Status = r.Status,
        SentAt = r.SentAt
    };
}
