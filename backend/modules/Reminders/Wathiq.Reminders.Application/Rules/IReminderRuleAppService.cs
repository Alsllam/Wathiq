using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Wathiq.Reminders.Rules;

/// <summary>
/// A SINGLETON resource: one rule per user, so neither method takes an id - the identity is the
/// caller. Routes become GET/PUT /api/reminders/rule with no {id} segment.
/// </summary>
public interface IReminderRuleAppService : IApplicationService
{
    Task<ReminderRuleDto> GetAsync();
    Task<ReminderRuleDto> UpdateAsync(UpdateReminderRuleDto input);
}
