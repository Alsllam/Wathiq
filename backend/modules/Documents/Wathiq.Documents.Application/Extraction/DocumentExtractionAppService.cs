using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Wathiq.Documents.Documents;
using Wathiq.Documents.Permissions;
using Wathiq.Shared.Extraction;

namespace Wathiq.Documents.Extraction;

/// <summary>
/// Closes the UC-01 loop: extract -> review -> confirm. The AI's output stays a draft in escrow
/// (an ExtractionResult row) until ConfirmAsync pushes it through the SAME domain methods a
/// manual edit uses - so ValidityPeriod's rules and the 2.4 reminder-resync event fire exactly
/// as if the user had typed the values (FR-DOC-005).
/// </summary>
[Authorize(DocumentsPermissions.Documents.Default)]
public class DocumentExtractionAppService : DocumentsAppServiceBase, IDocumentExtractionAppService
{
    private readonly IRepository<Document, Guid> _documents;
    private readonly IRepository<ExtractionResult, Guid> _results;
    private readonly IDocumentDataExtractor _extractor;   // the Shared seam - Ai stays invisible
    private readonly IDocumentAppService _documentAppService;

    public DocumentExtractionAppService(
        IRepository<Document, Guid> documents,
        IRepository<ExtractionResult, Guid> results,
        IDocumentDataExtractor extractor,
        IDocumentAppService documentAppService)
    {
        _documents = documents;
        _results = results;
        _extractor = extractor;
        _documentAppService = documentAppService;
    }

    [Authorize(DocumentsPermissions.Documents.Update)]
    public async Task<ExtractionProposalDto> ExtractAsync(Guid id, Guid attachmentId)
    {
        var document = await GetOwnedAsync(id);
        var attachment = document.Attachments.FirstOrDefault(a => a.Id == attachmentId)
                         ?? throw new EntityNotFoundException(typeof(Attachment), attachmentId);

        if (attachment.OcrText == null)
        {
            // 3.5's job hasn't landed (or the type isn't OCR-able) - tell the user, don't guess.
            throw new BusinessException(DocumentsErrorCodes.ExtractionNotReady);
        }

        DocumentDataProposal proposal;
        try
        {
            proposal = await _extractor.ExtractAsync(attachment.OcrText);
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            // Failure is data (3.8 queries failure rates) - but this UoW is about to roll back
            // with the exception, so the Failed row needs its own transaction (the ledger idiom).
            using (var uow = UnitOfWorkManager.Begin(new AbpUnitOfWorkOptions(), requiresNew: true))
            {
                await _results.InsertAsync(new ExtractionResult(
                    GuidGenerator.Create(), attachmentId, "ollama", "unavailable",
                    "extract-document@v1", rawJson: "", confidence: null, durationMs: 0, failed: true));
                await uow.CompleteAsync();
            }

            throw new BusinessException(DocumentsErrorCodes.ExtractionFailed, innerException: ex);
        }

        var result = await _results.InsertAsync(new ExtractionResult(
            GuidGenerator.Create(), attachmentId, proposal.Provider, proposal.Model,
            proposal.PromptVersion, proposal.RawJson, proposal.Confidence, proposal.DurationMs),
            autoSave: true);

        return ToDto(result, proposal);
    }

    public async Task<ExtractionProposalDto?> GetLatestAsync(Guid id, Guid attachmentId)
    {
        await GetOwnedAsync(id);   // ownership gate first - 404 semantics cover the child rows

        var query = (await _results.GetQueryableAsync())
            .Where(r => r.AttachmentId == attachmentId)
            .OrderByDescending(r => r.CreationTime);
        var result = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (result == null)
        {
            return null;
        }

        // Re-parse, never re-ask: same RawJson -> same proposal (and same warnings), model idle.
        return ToDto(result, result.Outcome == ExtractionOutcome.Failed
            ? new DocumentDataProposal()
            : _extractor.ParseStored(result.RawJson));
    }

    [Authorize(DocumentsPermissions.Documents.Update)]
    public async Task<DocumentDto> ConfirmAsync(Guid id, Guid extractionResultId, ConfirmExtractionDto input)
    {
        var document = await GetOwnedAsync(id);
        var result = await GetResultForAsync(document, extractionResultId);

        // Accepted vs Edited: compared against the VALIDATED proposal, so keeping a dropped
        // field empty still counts as accepting what the system actually proposed.
        var proposal = _extractor.ParseStored(result.RawJson);
        var untouched = input.Number == proposal.Number
                        && input.IssueDate == proposal.IssueDate
                        && input.ExpiryDate == proposal.ExpiryDate;
        if (untouched)
        {
            result.Accept();
        }
        else
        {
            result.MarkEdited();
        }

        // The same path a manual edit takes: domain validation + the expiry-changed event that
        // resyncs reminders (2.4) - extraction gets the whole 2.x machinery for free.
        document
            .SetNumber(input.Number)
            .SetValidity(new ValidityPeriod(input.IssueDate, input.ExpiryDate));

        await _documents.UpdateAsync(document, autoSave: true);
        return await _documentAppService.GetAsync(id);
    }

    [Authorize(DocumentsPermissions.Documents.Update)]
    public async Task RejectAsync(Guid id, Guid extractionResultId)
    {
        var document = await GetOwnedAsync(id);
        (await GetResultForAsync(document, extractionResultId)).Reject();
    }

    /// <summary>404 unless the result belongs to one of THIS document's attachments.</summary>
    private async Task<ExtractionResult> GetResultForAsync(Document document, Guid extractionResultId)
    {
        var result = await _results.GetAsync(extractionResultId);
        return document.Attachments.Any(a => a.Id == result.AttachmentId)
            ? result
            : throw new EntityNotFoundException(typeof(ExtractionResult), extractionResultId);
    }

    private async Task<Document> GetOwnedAsync(Guid id)
    {
        var document = await _documents.GetAsync(id);
        return document.OwnerUserId == CurrentUser.GetId()
            ? document
            : throw new EntityNotFoundException(typeof(Document), id);
    }

    private static ExtractionProposalDto ToDto(ExtractionResult result, DocumentDataProposal proposal) => new()
    {
        ExtractionResultId = result.Id,
        AttachmentId = result.AttachmentId,
        Number = proposal.Number,
        IssueDate = proposal.IssueDate,
        ExpiryDate = proposal.ExpiryDate,
        HolderName = proposal.HolderName,
        DocumentKind = proposal.DocumentKind,
        Confidence = result.Confidence,
        Warnings = proposal.Warnings,
        PromptVersion = result.PromptVersion,
        Model = result.Model,
        Outcome = result.Outcome,
        CreationTime = result.CreationTime
    };
}
