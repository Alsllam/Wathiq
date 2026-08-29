using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Wathiq.Documents.Documents;
using Wathiq.Shared.Files;
using Wathiq.Shared.Ocr;

namespace Wathiq.Documents.Ocr;

/// <summary>
/// Enqueue-once counterpart of 2.5's recurring dispatch: one upload = one job, run by whatever
/// engine the host wired behind IBackgroundJobManager (Hangfire). Idempotent by state, like the
/// dispatch: an already-OCR'd attachment is skipped, so a retry after a crash never double-works.
/// </summary>
// ITransientDependency does double duty: DI registration AND the hook ABP's job discovery
// listens on - without it the job never reaches AbpBackgroundJobOptions and enqueues go nowhere.
public class AttachmentOcrJob : AsyncBackgroundJob<AttachmentOcrArgs>, IUnitOfWorkEnabled, Volo.Abp.DependencyInjection.ITransientDependency
{
    private readonly IRepository<Document, Guid> _documents;
    private readonly IFileStore _fileStore;
    private readonly IOcrService _ocr;
    private readonly ILogger<AttachmentOcrJob> _logger;

    public AttachmentOcrJob(
        IRepository<Document, Guid> documents,
        IFileStore fileStore,
        IOcrService ocr,
        ILogger<AttachmentOcrJob> logger)
    {
        _documents = documents;
        _fileStore = fileStore;
        _ocr = ocr;
        _logger = logger;
    }

    // No [Authorize], no CurrentUser: a queue thread has no principal. The args are trusted
    // because only the post-commit handler creates them - ownership was checked at upload time.
    public override async Task ExecuteAsync(AttachmentOcrArgs args)
    {
        // FindAsync, not Get: the document may have been deleted between enqueue and execution -
        // for a background job that is a non-event, not an error worth a retry storm.
        var document = await _documents.FindAsync(args.DocumentId);
        var attachment = document?.Attachments.FirstOrDefault(a => a.Id == args.AttachmentId);
        if (document == null || attachment == null)
        {
            _logger.LogInformation("OCR skipped: attachment {AttachmentId} no longer exists.", args.AttachmentId);
            return;
        }

        if (attachment.OcrText != null)
        {
            return;   // retry or duplicate enqueue: work already done (idempotent by state)
        }

        await using var content = await _fileStore.GetAsync(DocumentConsts.AttachmentContainer, attachment.BlobKey);
        var text = await _ocr.ExtractTextAsync(content, attachment.MimeType);
        if (text == null)
        {
            // e.g. a PDF: recorded honestly as "not OCR-able today" (null), never as empty success.
            _logger.LogInformation("OCR skipped: {MimeType} is not readable by the OCR engine.", attachment.MimeType);
            return;
        }

        document.SetAttachmentOcrText(args.AttachmentId, text);
        await _documents.UpdateAsync(document, autoSave: true);
    }
}
