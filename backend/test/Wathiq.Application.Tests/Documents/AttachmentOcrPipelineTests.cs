using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Documents.Documents;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Holders;
using Wathiq.Documents.Ocr;
using Xunit;

namespace Wathiq.Documents;

/* Stage one of UC-01 with the queue and engine faked: upload -> event -> enqueue (asserted on
 * the recording manager), then the job invoked directly (the 2.5 discipline) -> OcrText via the
 * aggregate. Concrete class lives in EntityFrameworkCore.Tests. */
public abstract class AttachmentOcrPipelineTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IDocumentAppService _documents;
    private readonly IHolderAppService _holders;
    private readonly IDocumentTypeAppService _types;
    private readonly ICurrentPrincipalAccessor _principalAccessor;
    private readonly RecordingBackgroundJobManager _jobManager;
    private readonly FakeOcrService _ocr;

    protected AttachmentOcrPipelineTests()
    {
        _documents = GetRequiredService<IDocumentAppService>();
        _holders = GetRequiredService<IHolderAppService>();
        _types = GetRequiredService<IDocumentTypeAppService>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        _jobManager = GetRequiredService<RecordingBackgroundJobManager>();
        _ocr = GetRequiredService<FakeOcrService>();

        // The recorders are singletons shared by every test in the collection; counting without
        // clearing made assertions depend on run order (caught live: 1 red on the first run).
        _jobManager.Enqueued.Clear();
        _ocr.Calls.Clear();
    }

    private IDisposable ActAs(Guid userId) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "amina")
        ], "test")));

    private async Task<(Guid DocumentId, Guid AttachmentId)> UploadAsync(string fileName, string mimeType)
    {
        var self = (await _holders.GetListAsync()).Items.Single(h => h.IsSelf);
        var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");
        var doc = await _documents.CreateAsync(new CreateDocumentDto
        {
            HolderId = self.Id,
            DocumentTypeId = passport.Id,
            ExpiryDate = new DateOnly(2036, 3, 1)
        });

        var uploaded = await _documents.UploadAttachmentAsync(doc.Id,
            new RemoteStreamContent(new MemoryStream([1, 2, 3, 4]), fileName, mimeType));
        return (doc.Id, uploaded.Id);
    }

    [Fact]
    public async Task Upload_Enqueues_One_Ocr_Job_For_The_Attachment()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var (docId, attachmentId) = await UploadAsync("scan.png", "image/png");

            // The handler enqueued post-commit (OnCompleted) - by now the UoW is complete.
            var args = _jobManager.Enqueued.OfType<AttachmentOcrArgs>()
                .Where(a => a.AttachmentId == attachmentId).ShouldHaveSingleItem();
            args.DocumentId.ShouldBe(docId);
        }
    }

    [Fact]
    public async Task Job_Fills_OcrText_And_Is_Idempotent()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var (docId, attachmentId) = await UploadAsync("scan.png", "image/png");
            _ocr.TextToReturn = "جواز سفر P-102030 EXPIRY 2036-03-01";
            var args = new AttachmentOcrArgs { DocumentId = docId, AttachmentId = attachmentId };

            var job = GetRequiredService<AttachmentOcrJob>();
            await job.ExecuteAsync(args);
            await job.ExecuteAsync(args);   // retry/duplicate delivery

            var document = await GetRequiredService<IRepository<Document, Guid>>().GetAsync(docId);
            document.Attachments.Single().OcrText.ShouldBe("جواز سفر P-102030 EXPIRY 2036-03-01");
            _ocr.Calls.Count.ShouldBe(1);   // second run stopped at "already OCR'd"
        }
    }

    [Fact]
    public async Task Job_Leaves_OcrText_Null_When_The_Engine_Cannot_Read_The_Type()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var (docId, attachmentId) = await UploadAsync("contract.pdf", "application/pdf");

            await GetRequiredService<AttachmentOcrJob>()
                .ExecuteAsync(new AttachmentOcrArgs { DocumentId = docId, AttachmentId = attachmentId });

            var document = await GetRequiredService<IRepository<Document, Guid>>().GetAsync(docId);
            document.Attachments.Single().OcrText.ShouldBeNull();   // "couldn't read" is not ""
        }
    }
}
