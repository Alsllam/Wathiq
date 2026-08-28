using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.Jobs;

/// <summary>
/// The nightly dispatch (FR-REM-002/003). Deliberately a plain DI class with no Hangfire types:
/// the host schedules RunAsync with a cron; tests call it directly. Safe to run twice because
/// sending is gated by the Pending status and recorded as a transition on the same row that the
/// unique index (2.3) keeps singular - the second run simply finds nothing Pending.
/// </summary>
public class ReminderDispatchJob : IUnitOfWorkEnabled, Volo.Abp.DependencyInjection.ITransientDependency
{
    private readonly IRepository<Reminder, Guid> _reminders;
    private readonly IRepository<ReminderRule, Guid> _rules;
    private readonly IEnumerable<IReminderChannel> _channels;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<ReminderDispatchJob> _logger;

    public ReminderDispatchJob(
        IRepository<Reminder, Guid> reminders,
        IRepository<ReminderRule, Guid> rules,
        IEnumerable<IReminderChannel> channels,
        IClock clock,
        IGuidGenerator guidGenerator,
        ILogger<ReminderDispatchJob> logger)
    {
        _reminders = reminders;
        _rules = rules;
        _channels = channels;
        _clock = clock;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    // virtual: Hangfire calls through the Castle proxy, so IUnitOfWorkEnabled opens the UoW
    // this method needs (a Hangfire thread has no ambient request UoW).
    public virtual async Task RunAsync()
    {
        var utcNow = _clock.Now;

        // Widest "today" on Earth (UTC+14) as the SQL pre-filter; the per-user time zone
        // decides for real below. Uses IX_Reminder_Status_DueDate.
        var latestTodayAnywhere = DateOnly.FromDateTime(utcNow.AddHours(14));
        var candidates = await _reminders.GetListAsync(r =>
            r.Status == ReminderStatus.Pending && r.DueDate <= latestTodayAnywhere);

        if (candidates.Count == 0)
        {
            return;
        }

        var userIds = candidates.Select(r => r.UserId).Distinct().ToList();
        var ruleByUser = (await _rules.GetListAsync(r => userIds.Contains(r.UserId)))
            .ToDictionary(r => r.UserId);

        var sent = 0;
        var failed = 0;

        foreach (var reminder in candidates)
        {
            // A reminder without a rule cannot exist (sync creates the rule first) - but a job
            // must be paranoid about its own data; skipping beats crashing the whole run.
            if (!ruleByUser.TryGetValue(reminder.UserId, out var rule))
            {
                _logger.LogWarning("Reminder {ReminderId} has no rule for user {UserId}; skipped.", reminder.Id, reminder.UserId);
                continue;
            }

            if (reminder.DueDate > ReminderScheduler.TodayIn(rule.TimeZoneId, utcNow))
            {
                continue;   // due tomorrow in THIS user's time zone; the pre-filter was just coarse
            }

            foreach (var channel in _channels.Where(c => rule.Channels.HasFlag(c.Channel)))
            {
                try
                {
                    await channel.SendAsync(reminder, rule);
                    reminder.RecordAttempt(_guidGenerator.Create(), channel.Channel, _clock.Now, succeeded: true);
                    sent++;
                }
                catch (Exception ex)
                {
                    // One bad address must not kill the run: the exception becomes a Failed row
                    // (error text truncated by DeliveryLog itself) and the loop continues.
                    reminder.RecordAttempt(_guidGenerator.Create(), channel.Channel, _clock.Now, succeeded: false, error: ex.Message);
                    failed++;
                    _logger.LogError(ex, "Reminder {ReminderId} delivery via {Channel} failed.", reminder.Id, channel.Channel);
                }
            }
        }

        _logger.LogInformation("Reminder dispatch: {Sent} sent, {Failed} failed of {Candidates} candidates.", sent, failed, candidates.Count);
    }
}
