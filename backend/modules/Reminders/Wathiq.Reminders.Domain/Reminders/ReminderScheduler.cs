using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Wathiq.Reminders.Rules;

namespace Wathiq.Reminders.Reminders;

/// <summary>
/// Owns the scheduling rule "which reminders should exist for a document right now".
/// The date math is static and clock-free (testable without DI); only the sync touches IO.
/// </summary>
public class ReminderScheduler : DomainService
{
    private readonly IRepository<Reminder, Guid> _reminders;
    private readonly ReminderRuleManager _ruleManager;

    public ReminderScheduler(IRepository<Reminder, Guid> reminders, ReminderRuleManager ruleManager)
    {
        _reminders = reminders;
        _ruleManager = ruleManager;
    }

    /// <summary>Expiry − offset, keeping only dates not already behind the user. Pure: no clock, no IO.</summary>
    public static IReadOnlyList<(int OffsetDays, DateOnly DueDate)> ComputeSchedule(
        ReminderOffsets offsets, DateOnly? expiryDate, DateOnly today)
    {
        if (!expiryDate.HasValue)
        {
            return [];
        }

        return offsets.Days
            .Select(d => (OffsetDays: d, DueDate: expiryDate.Value.AddDays(-d)))
            .Where(x => x.DueDate >= today)   // >=: a reminder due today still goes out tonight
            .ToList();
    }

    /// <summary>"Today" is a per-user fact: 21:30 UTC is already tomorrow in Riyadh (UTC+3).</summary>
    public static DateOnly TodayIn(string timeZoneId, DateTime utcNow) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcNow, DateTimeKind.Utc),
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)));

    /// <summary>
    /// Re-derives every document's schedule after the rule changed (new offsets). No call to the
    /// Documents module: each row is self-describing - DueDate + OffsetDays IS the expiry date -
    /// so the module can rebuild its own schedule from its own table (ADR-001 kept intact).
    /// </summary>
    public async Task ResyncForUserAsync(Guid userId)
    {
        var reminders = await _reminders.GetListAsync(r => r.UserId == userId);

        foreach (var documentGroup in reminders.GroupBy(r => r.DocumentId))
        {
            // Max guards against stale cancelled rows from an older, shorter expiry.
            var expiry = documentGroup.Max(r => r.DueDate.AddDays(r.OffsetDays));
            await SyncForDocumentAsync(userId, documentGroup.Key, expiry);
        }
    }

    /// <summary>
    /// Upserts toward the desired schedule. Rows are REUSED (unique DocumentId+OffsetDays):
    /// matching pending rows stay, changed dates re-arm via Reschedule, unwanted rows Cancel -
    /// never deleted, so history and the unique index both survive. Runs inside the caller's
    /// unit of work; tracked entities flush on commit without explicit UpdateAsync.
    /// </summary>
    public async Task SyncForDocumentAsync(Guid userId, Guid documentId, DateOnly? expiryDate)
    {
        var rule = await _ruleManager.EnsureForUserAsync(userId);
        // Clock is configured to UTC host-wide (DB6); TodayIn re-asserts the kind defensively.
        var today = TodayIn(rule.TimeZoneId, Clock.Now);

        var desired = ComputeSchedule(rule.Offsets, expiryDate, today)
            .ToDictionary(x => x.OffsetDays, x => x.DueDate);

        var existing = await _reminders.GetListAsync(r => r.DocumentId == documentId);

        foreach (var reminder in existing)
        {
            if (desired.Remove(reminder.OffsetDays, out var dueDate))
            {
                // Sent for this exact date stays Sent; anything else re-arms for the (new) date.
                if (reminder.DueDate != dueDate || reminder.Status is ReminderStatus.Cancelled or ReminderStatus.Failed)
                {
                    reminder.Reschedule(dueDate);
                }
            }
            else
            {
                reminder.Cancel();   // offset removed from the rule, or the date slid into the past
            }
        }

        foreach (var (offsetDays, dueDate) in desired)
        {
            await _reminders.InsertAsync(new Reminder(GuidGenerator.Create(), userId, documentId, offsetDays, dueDate));
        }
    }
}
