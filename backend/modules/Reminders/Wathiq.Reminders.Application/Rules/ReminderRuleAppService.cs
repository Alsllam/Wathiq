using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Users;
using Wathiq.Reminders.Reminders;

namespace Wathiq.Reminders.Rules;

[Authorize(RemindersPermissions.Rule.Default)]
public class ReminderRuleAppService : RemindersAppServiceBase, IReminderRuleAppService
{
    private readonly ReminderRuleManager _ruleManager;
    private readonly ReminderScheduler _scheduler;

    public ReminderRuleAppService(ReminderRuleManager ruleManager, ReminderScheduler scheduler)
    {
        _ruleManager = ruleManager;
        _scheduler = scheduler;
    }

    public async Task<ReminderRuleDto> GetAsync()
    {
        // First GET materialises the default rule - same first-use pattern as holders (1.6).
        return ToDto(await _ruleManager.EnsureForUserAsync(CurrentUser.GetId()));
    }

    [Authorize(RemindersPermissions.Rule.Update)]
    public async Task<ReminderRuleDto> UpdateAsync(UpdateReminderRuleDto input)
    {
        var rule = await _ruleManager.EnsureForUserAsync(CurrentUser.GetId());

        rule.SetOffsets(new ReminderOffsets(input.OffsetsDays))   // value object validates the set
            .SetChannels(input.Channels)
            .SetQuietHours(input.QuietFrom, input.QuietTo)
            .SetTimeZone(input.TimeZoneId);

        // New offsets/zone change WHICH reminders should exist - re-derive them now, in this UoW,
        // so the user's next look at "upcoming" already matches the settings they just saved.
        await _scheduler.ResyncForUserAsync(rule.UserId);

        return ToDto(rule);
    }

    private static ReminderRuleDto ToDto(ReminderRule r) => new()
    {
        OffsetsDays = r.Offsets.Days.ToArray(),
        Channels = r.Channels,
        QuietFrom = r.QuietFrom,
        QuietTo = r.QuietTo,
        TimeZoneId = r.TimeZoneId
    };
}
