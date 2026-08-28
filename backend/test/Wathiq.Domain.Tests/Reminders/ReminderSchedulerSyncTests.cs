using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;
using Xunit;

namespace Wathiq.Reminders;

/* Sync against a real database: proves row REUSE under the unique index - reschedule and cancel
 * never insert duplicates. Concrete class in EFCore.Tests (SQLite). */
public abstract class ReminderSchedulerSyncTests<TStartupModule> : WathiqDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ReminderScheduler _scheduler;
    private readonly IRepository<Reminder, Guid> _reminders;

    protected ReminderSchedulerSyncTests()
    {
        _scheduler = GetRequiredService<ReminderScheduler>();
        _reminders = GetRequiredService<IRepository<Reminder, Guid>>();
    }

    [Fact]
    public async Task Sync_Creates_Then_Reuses_Then_Cancels()
    {
        var userId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1);

        // First sync: default rule materialises, four rows appear.
        await WithUnitOfWorkAsync(() => _scheduler.SyncForDocumentAsync(userId, documentId, expiry));
        var rows = await _reminders.GetListAsync(r => r.DocumentId == documentId);
        rows.Count.ShouldBe(4);
        rows.ShouldAllBe(r => r.Status == ReminderStatus.Pending);

        // Renewal: same document, expiry five years later. Same four rows, re-armed - not eight.
        await WithUnitOfWorkAsync(() => _scheduler.SyncForDocumentAsync(userId, documentId, expiry.AddYears(5)));
        rows = await _reminders.GetListAsync(r => r.DocumentId == documentId);
        rows.Count.ShouldBe(4);
        rows.Single(r => r.OffsetDays == 90).DueDate.ShouldBe(expiry.AddYears(5).AddDays(-90));

        // Expiry removed: everything pending is cancelled, nothing deleted.
        await WithUnitOfWorkAsync(() => _scheduler.SyncForDocumentAsync(userId, documentId, null));
        rows = await _reminders.GetListAsync(r => r.DocumentId == documentId);
        rows.Count.ShouldBe(4);
        rows.ShouldAllBe(r => r.Status == ReminderStatus.Cancelled);
    }

    [Fact]
    public async Task Running_The_Same_Sync_Twice_Changes_Nothing()
    {
        var userId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(6);

        await WithUnitOfWorkAsync(() => _scheduler.SyncForDocumentAsync(userId, documentId, expiry));
        await WithUnitOfWorkAsync(() => _scheduler.SyncForDocumentAsync(userId, documentId, expiry));

        // Idempotent by shape: the unique index would have thrown on any duplicate insert.
        (await _reminders.GetListAsync(r => r.DocumentId == documentId)).Count.ShouldBe(4);
    }
}
