using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Documents;
using Wathiq.Documents.Holders;
using Wathiq.Reminders.Jobs;
using Wathiq.Reminders.Reminders;
using Xunit;

namespace Wathiq.Reminders;

/* The 2.8 coverage-audit test: the one FR-REM path no earlier test pinned - a SENT reminder row
 * re-armed by a renewal and sent AGAIN, with delivery history accumulating on the same
 * uniquely-indexed row. Create -> dispatch -> renew -> dispatch, all through real services. */
public abstract class ReminderEndToEndTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDocumentAppService _documents;
    private readonly IHolderAppService _holders;
    private readonly IDocumentTypeAppService _types;
    private readonly ReminderDispatchJob _job;
    private readonly IRepository<Reminder, Guid> _reminders;
    private readonly FakeReminderChannel _channel;
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    protected ReminderEndToEndTests()
    {
        _documents = GetRequiredService<IDocumentAppService>();
        _holders = GetRequiredService<IHolderAppService>();
        _types = GetRequiredService<IDocumentTypeAppService>();
        _job = GetRequiredService<ReminderDispatchJob>();
        _reminders = GetRequiredService<IRepository<Reminder, Guid>>();
        _channel = GetRequiredService<FakeReminderChannel>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable ActAs(Guid userId) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "amina")
        ], "test")));

    [Fact]
    public async Task A_Sent_Reminder_Rearms_On_Renewal_And_Sends_Again()
    {
        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Guid documentId;

        using (ActAs(userId))
        {
            var self = (await _holders.GetListAsync()).Items.Single();
            var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");

            // Expiry tomorrow: only the 1-day offset survives ComputeSchedule, and it is due TODAY.
            documentId = (await _documents.CreateAsync(new CreateDocumentDto
            {
                HolderId = self.Id, DocumentTypeId = passport.Id, ExpiryDate = today.AddDays(1)
            })).Id;
        }

        await _job.RunAsync();
        var oneDayRow = (await _reminders.GetListAsync(r => r.DocumentId == documentId))
            .Single(r => r.OffsetDays == 1);
        oneDayRow.Status.ShouldBe(ReminderStatus.Sent);

        // Renewal to +30 days: the SENT 1-day row must re-arm (same row - unique index), and the
        // 30-day offset becomes due today (expiry - 30 == today).
        using (ActAs(userId))
        {
            await _documents.RenewAsync(documentId, new RenewDocumentDto { ExpiryDate = today.AddDays(30) });
        }

        var rows = await _reminders.GetListAsync(r => r.DocumentId == documentId);
        rows.Count.ShouldBeLessThanOrEqualTo(4);   // never a 5th row for the same document
        var rearmed = rows.Single(r => r.OffsetDays == 1);
        rearmed.Id.ShouldBe(oneDayRow.Id);         // literally the same row, re-armed
        rearmed.Status.ShouldBe(ReminderStatus.Pending);
        rearmed.SentAt.ShouldBeNull();

        await _job.RunAsync();

        (await _reminders.GetListAsync(r => r.DocumentId == documentId))
            .Single(r => r.OffsetDays == 30).Status.ShouldBe(ReminderStatus.Sent);

        // Two real sends across the lifecycle; the fake channel saw both dispatch rounds.
        _channel.SentReminderIds.Count.ShouldBe(2);

        // FR-REM-005: history accumulated - the first send's log survives on the re-armed row.
        await WithUnitOfWorkAsync(async () =>
        {
            var withLogs = (await _reminders.WithDetailsAsync(r => r.DeliveryLogs))
                .Single(r => r.Id == oneDayRow.Id);
            withLogs.DeliveryLogs.Count.ShouldBe(1);
        });
    }
}
