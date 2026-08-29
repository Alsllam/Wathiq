using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Documents.Documents;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Extraction;
using Wathiq.Documents.Holders;
using Wathiq.Documents.Ocr;
using Xunit;

namespace Wathiq.Documents;

/* UC-01's tail end to end (model scripted): OCR'd attachment -> extract -> escrow row ->
 * confirm/edit/reject -> document fields + reminders resync. Concrete class in EFCore.Tests. */
public abstract class ExtractionFlowTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDocumentAppService _documents;
    private readonly IDocumentExtractionAppService _extraction;
    private readonly IHolderAppService _holders;
    private readonly IDocumentTypeAppService _types;
    private readonly IRepository<ExtractionResult, Guid> _results;
    private readonly IRepository<Reminders.Reminders.Reminder, Guid> _reminders;
    private readonly ICurrentPrincipalAccessor _principalAccessor;
    private readonly FakeDocumentDataExtractor _fakeExtractor;

    protected ExtractionFlowTests()
    {
        _documents = GetRequiredService<IDocumentAppService>();
        _extraction = GetRequiredService<IDocumentExtractionAppService>();
        _holders = GetRequiredService<IHolderAppService>();
        _types = GetRequiredService<IDocumentTypeAppService>();
        _results = GetRequiredService<IRepository<ExtractionResult, Guid>>();
        _reminders = GetRequiredService<IRepository<Reminders.Reminders.Reminder, Guid>>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        _fakeExtractor = GetRequiredService<FakeDocumentDataExtractor>();
        _fakeExtractor.NextProposal = FakeDocumentDataExtractor.NewProposal();
        _fakeExtractor.NextException = null;
    }

    private IDisposable ActAs(Guid userId) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "amina")
        ], "test")));

    private async Task<(Guid DocumentId, Guid AttachmentId)> CreateOcrReadyDocumentAsync()
    {
        var self = (await _holders.GetListAsync()).Items.Single(h => h.IsSelf);
        var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");
        var doc = await _documents.CreateAsync(new CreateDocumentDto
        {
            HolderId = self.Id,
            DocumentTypeId = passport.Id
        });
        var uploaded = await _documents.UploadAttachmentAsync(doc.Id,
            new RemoteStreamContent(new MemoryStream([1, 2, 3]), "scan.png", "image/png"));

        // Run 3.5's job directly (the queue is a recorder in tests) so OcrText is filled.
        await GetRequiredService<AttachmentOcrJob>().ExecuteAsync(
            new AttachmentOcrArgs { DocumentId = doc.Id, AttachmentId = uploaded.Id });

        return (doc.Id, uploaded.Id);
    }

    [Fact]
    public async Task Extract_Refuses_An_Attachment_Without_OcrText()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var self = (await _holders.GetListAsync()).Items.Single(h => h.IsSelf);
            var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");
            var doc = await _documents.CreateAsync(new CreateDocumentDto { HolderId = self.Id, DocumentTypeId = passport.Id });
            var uploaded = await _documents.UploadAttachmentAsync(doc.Id,
                new RemoteStreamContent(new MemoryStream([9]), "scan.png", "image/png"));

            (await Should.ThrowAsync<BusinessException>(() => _extraction.ExtractAsync(doc.Id, uploaded.Id)))
                .Code.ShouldBe(DocumentsErrorCodes.ExtractionNotReady);
        }
    }

    [Fact]
    public async Task Extract_Stores_A_Proposed_Row_And_Returns_The_Proposal()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var (docId, attachmentId) = await CreateOcrReadyDocumentAsync();

            var dto = await _extraction.ExtractAsync(docId, attachmentId);

            dto.Number.ShouldBe("P-102030");
            dto.ExpiryDate.ShouldBe(new DateOnly(2036, 3, 1));
            dto.Outcome.ShouldBe(ExtractionOutcome.Proposed);
            _fakeExtractor.LastOcrText.ShouldNotBeNullOrWhiteSpace();   // fed the OCR text, not the blob

            (await _results.GetAsync(dto.ExtractionResultId)).Outcome.ShouldBe(ExtractionOutcome.Proposed);
            (await _extraction.GetLatestAsync(docId, attachmentId))!.ExtractionResultId.ShouldBe(dto.ExtractionResultId);
        }
    }

    [Fact]
    public async Task Confirm_Unchanged_Accepts_Applies_And_Resyncs_Reminders()
    {
        var userId = Guid.NewGuid();
        using (ActAs(userId))
        {
            var (docId, attachmentId) = await CreateOcrReadyDocumentAsync();
            var dto = await _extraction.ExtractAsync(docId, attachmentId);

            var confirmed = await _extraction.ConfirmAsync(docId, dto.ExtractionResultId, new ConfirmExtractionDto
            {
                Number = dto.Number, IssueDate = dto.IssueDate, ExpiryDate = dto.ExpiryDate
            });

            confirmed.Number.ShouldBe("P-102030");
            confirmed.ExpiryDate.ShouldBe(new DateOnly(2036, 3, 1));
            (await _results.GetAsync(dto.ExtractionResultId)).Outcome.ShouldBe(ExtractionOutcome.Accepted);

            // The 2.4 event fired: reminders exist for the confirmed expiry, and extraction
            // never mentioned the Reminders module.
            (await _reminders.GetListAsync(r => r.DocumentId == docId)).ShouldNotBeEmpty();
        }
    }

    [Fact]
    public async Task Confirm_With_Changes_Is_Edited()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var (docId, attachmentId) = await CreateOcrReadyDocumentAsync();
            var dto = await _extraction.ExtractAsync(docId, attachmentId);

            await _extraction.ConfirmAsync(docId, dto.ExtractionResultId, new ConfirmExtractionDto
            {
                Number = dto.Number, IssueDate = dto.IssueDate,
                ExpiryDate = new DateOnly(2035, 1, 1)   // the user corrected the model
            });

            (await _results.GetAsync(dto.ExtractionResultId)).Outcome.ShouldBe(ExtractionOutcome.Edited);
        }
    }

    [Fact]
    public async Task Reject_Concludes_Without_Touching_The_Document()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var (docId, attachmentId) = await CreateOcrReadyDocumentAsync();
            var dto = await _extraction.ExtractAsync(docId, attachmentId);

            await _extraction.RejectAsync(docId, dto.ExtractionResultId);

            (await _results.GetAsync(dto.ExtractionResultId)).Outcome.ShouldBe(ExtractionOutcome.Rejected);
            (await _documents.GetAsync(docId)).Number.ShouldBeNull();   // untouched
        }
    }

    [Fact]
    public async Task A_Dead_Model_Leaves_A_Failed_Row_Behind()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var (docId, attachmentId) = await CreateOcrReadyDocumentAsync();
            _fakeExtractor.NextException = new HttpRequestException("connection refused");

            (await Should.ThrowAsync<BusinessException>(() => _extraction.ExtractAsync(docId, attachmentId)))
                .Code.ShouldBe(DocumentsErrorCodes.ExtractionFailed);

            // The requiresNew UoW did its job: the evidence survived the rollback it rode along with.
            (await _results.GetListAsync(r => r.AttachmentId == attachmentId))
                .ShouldHaveSingleItem().Outcome.ShouldBe(ExtractionOutcome.Failed);
        }
    }
}
