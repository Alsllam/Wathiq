using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Wathiq.Reminders.Rules;

/// <summary>
/// Same first-use pattern as HolderManager: Reminders cannot hook user registration without
/// crossing into Identity, so the default rule materialises the first time anything needs it.
/// </summary>
public class ReminderRuleManager : DomainService
{
    private readonly IRepository<ReminderRule, Guid> _rules;

    public ReminderRuleManager(IRepository<ReminderRule, Guid> rules)
    {
        _rules = rules;
    }

    public async Task<ReminderRule> EnsureForUserAsync(Guid userId)
    {
        var existing = await _rules.FindAsync(r => r.UserId == userId);
        if (existing != null)
        {
            return existing;
        }

        var rule = new ReminderRule(
            GuidGenerator.Create(),
            userId,
            ReminderOffsets.Default,
            ReminderChannels.Email,               // push joins in Phase 6
            ReminderRuleConsts.DefaultTimeZoneId);

        return await _rules.InsertAsync(rule, autoSave: true);
    }
}
