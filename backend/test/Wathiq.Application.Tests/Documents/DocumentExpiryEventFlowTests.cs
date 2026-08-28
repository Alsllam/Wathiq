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
using Wathiq.Reminders.Reminders;
using Xunit;

namespace Wathiq.Documents;

/* The whole FR-REM-004 chain across the module boundary: DocumentAppService -> local event ->
 * Reminders handler -> reminder rows. No module references the other; only Shared's Eto travels. */
public abstract class DocumentExpiryEventFlowTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDocumentAppService _documents;
    private readonly IHolderAppService _holders;
    private readonly IDocumentTypeAppService _types;
    private readonly IRepository<Reminder, Guid> _reminders;
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    protected DocumentExpiryEventFlowTests()
    {
        _documents = GetRequiredService<IDocumentAppService>();
        _holders = GetRequiredService<IHolderAppService>();
        _types = GetRequiredService<IDocumentTypeAppService>();
        _reminders = GetRequiredService<IRepository<Reminder, Guid>>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable ActAs(Guid userId) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "amina")
        ], "test")));

    [Fact]
    public async Task Document_Lifecycle_Drives_Reminders_Through_The_Event()
    {
        var userId = Guid.NewGuid();
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1);
        Guid documentId;

        using (ActAs(userId))
        {
            var self = (await _holders.GetListAsync()).Items.Single();
            var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");

            // Create: the aggregate's local event fires inside the same save.
            documentId = (await _documents.CreateAsync(new CreateDocumentDto
            {
                HolderId = self.Id,
                DocumentTypeId = passport.Id,
                IssueDate = expiry.AddYears(-10),
                ExpiryDate = expiry
            })).Id;
        }

        var rows = await _reminders.GetListAsync(r => r.DocumentId == documentId);
        rows.Count.ShouldBe(4);   // default rule 90/30/7/1, all future
        rows.ShouldAllBe(r => r.UserId == userId && r.Status == ReminderStatus.Pending);

        // Renew: same rows re-armed for the new expiry - the unique index forbids duplicates.
        using (ActAs(userId))
        {
            await _documents.RenewAsync(documentId, new RenewDocumentDto { ExpiryDate = expiry.AddYears(5) });
        }

        rows = await _reminders.GetListAsync(r => r.DocumentId == documentId);
        rows.Count.ShouldBe(4);
        rows.Single(r => r.OffsetDays == 1).DueDate.ShouldBe(expiry.AddYears(5).AddDays(-1));

        // Delete: reminders cancel in the same transaction, rows and history survive.
        using (ActAs(userId))
        {
            await _documents.DeleteAsync(documentId);
        }

        rows = await _reminders.GetListAsync(r => r.DocumentId == documentId);
        rows.ShouldAllBe(r => r.Status == ReminderStatus.Cancelled);
    }

    [Fact]
    public async Task Updating_Without_Changing_Dates_Publishes_Nothing()
    {
        var userId = Guid.NewGuid();
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(6);
        Guid documentId;

        using (ActAs(userId))
        {
            var self = (await _holders.GetListAsync()).Items.Single();
            var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");
            documentId = (await _documents.CreateAsync(new CreateDocumentDto
            {
                HolderId = self.Id,
                DocumentTypeId = passport.Id,
                ExpiryDate = expiry
            })).Id;

            var before = (await _reminders.GetListAsync(r => r.DocumentId == documentId))
                .ToDictionary(r => r.Id, r => r.LastModificationTime);

            // Same dates, new notes: the value-object equality gate must keep reminders untouched.
            await _documents.UpdateAsync(documentId, new UpdateDocumentDto { ExpiryDate = expiry, Notes = "renew soon" });

            var after = await _reminders.GetListAsync(r => r.DocumentId == documentId);
            after.ShouldAllBe(r => r.LastModificationTime == before[r.Id]);
        }
    }
}
