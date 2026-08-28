using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Holders;
using Wathiq.Documents.Permissions;
using Wathiq.Shared.Files;

namespace Wathiq.Documents.Documents;

[Authorize(DocumentsPermissions.Documents.Default)]
public class DocumentAppService : DocumentsAppServiceBase, IDocumentAppService
{
    private readonly IRepository<Document, Guid> _documents;
    private readonly IRepository<Holder, Guid> _holders;
    private readonly IRepository<DocumentType, Guid> _documentTypes;
    private readonly IFileStore _fileStore;

    public DocumentAppService(
        IRepository<Document, Guid> documents,
        IRepository<Holder, Guid> holders,
        IRepository<DocumentType, Guid> documentTypes,
        IFileStore fileStore)
    {
        _documents = documents;
        _holders = holders;
        _documentTypes = documentTypes;
        _fileStore = fileStore;
    }

    public async Task<DocumentDto> GetAsync(Guid id)
    {
        return ToDto(await GetOwnedAsync(id), Today());
    }

    public async Task<PagedResultDto<DocumentDto>> GetListAsync(GetDocumentListInput input)
    {
        // GetQueryableAsync + AsyncExecuter: compose LINQ in the service but keep the async
        // execution provider-agnostic (EF here, in-memory in unit tests) - the ABP idiom.
        var query = (await _documents.GetQueryableAsync())
            .Where(d => d.OwnerUserId == CurrentUser.GetId())
            .WhereIf(input.HolderId.HasValue, d => d.HolderId == input.HolderId)
            .WhereIf(input.DocumentTypeId.HasValue, d => d.DocumentTypeId == input.DocumentTypeId)
            .WhereIf(input.Status.HasValue, d => d.Status == input.Status);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var page = await AsyncExecuter.ToListAsync(query
            // Expiry timeline order: soonest expiry first, documents without one last.
            .OrderBy(d => d.Validity.ExpiryDate == null)
            .ThenBy(d => d.Validity.ExpiryDate)
            .ThenByDescending(d => d.CreationTime)
            .PageBy(input));

        var today = Today();
        return new PagedResultDto<DocumentDto>(totalCount, page.Select(d => ToDto(d, today)).ToList());
    }

    [Authorize(DocumentsPermissions.Documents.Create)]
    public async Task<DocumentDto> CreateAsync(CreateDocumentDto input)
    {
        // The holder id comes from the client: re-check it belongs to the caller, otherwise a
        // guessed id would let one user file documents under another user's holder.
        var holder = await _holders.GetAsync(input.HolderId);
        if (holder.UserId != CurrentUser.GetId())
        {
            throw new BusinessException(DocumentsErrorCodes.HolderNotOwned);
        }

        var type = await _documentTypes.GetAsync(input.DocumentTypeId);
        if (!type.IsActive)
        {
            throw new BusinessException(DocumentsErrorCodes.DocumentTypeNotActive);
        }

        var document = new Document(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            holder.Id,
            type.Id,
            // The value object validates the pair here; an invalid range never reaches EF.
            new ValidityPeriod(input.IssueDate, input.ExpiryDate),
            input.Number,
            input.Notes);

        await _documents.InsertAsync(document, autoSave: true);
        return ToDto(document, Today());
    }

    [Authorize(DocumentsPermissions.Documents.Update)]
    public async Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto input)
    {
        var document = await GetOwnedAsync(id);

        document
            .SetNumber(input.Number)
            .SetNotes(input.Notes)
            .SetValidity(new ValidityPeriod(input.IssueDate, input.ExpiryDate));

        await _documents.UpdateAsync(document, autoSave: true);
        return ToDto(document, Today());
    }

    // Renewal is Update-level work on your own document - no extra permission name for it.
    [Authorize(DocumentsPermissions.Documents.Update)]
    public async Task<DocumentDto> RenewAsync(Guid id, RenewDocumentDto input)
    {
        var document = await GetOwnedAsync(id);

        document.MarkRenewed(new ValidityPeriod(input.IssueDate, input.ExpiryDate));

        await _documents.UpdateAsync(document, autoSave: true);
        return ToDto(document, Today());
    }

    [Authorize(DocumentsPermissions.Documents.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var document = await GetOwnedAsync(id);

        // Soft delete (FullAudited): the row is flagged, attachments and their files stay for
        // the Phase 8 "delete my data" hard-purge to handle. Reminders must stop, though (2.4).
        document.PublishRemindersStop();
        await _documents.DeleteAsync(document, autoSave: true);
    }

    [Authorize(DocumentsPermissions.Documents.Update)]
    public async Task<AttachmentDto> UploadAttachmentAsync(Guid id, IRemoteStreamContent file)
    {
        var document = await GetOwnedAsync(id);

        var mimeType = file.ContentType?.ToLowerInvariant();
        if (mimeType == null || !DocumentConsts.AllowedMimeTypes.Contains(mimeType))
        {
            // Rejected before a single byte is stored - the allow-list is the first gate.
            throw new BusinessException(DocumentsErrorCodes.UnsupportedFileType)
                .WithData("ContentType", file.ContentType ?? "-");
        }

        // Buffer once so sha256 and the store read the same bytes (upload streams are generally
        // not seekable). Size cap: Kestrel bounds the request; IFileStore enforces MaxSizeBytes.
        using var buffer = new MemoryStream();
        await file.GetStream().CopyToAsync(buffer);
        var sha256 = SHA256.HashData(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));

        buffer.Position = 0;
        var blobKey = await _fileStore.SaveAsync(
            DocumentConsts.AttachmentContainer, file.FileName ?? "upload", buffer);

        // Compensation: the blob exists but the row may never commit - if this UoW fails, remove
        // the file again so a rolled-back upload leaves no orphan on disk.
        UnitOfWorkManager.Current!.Failed += async (_, _) =>
            await _fileStore.DeleteAsync(DocumentConsts.AttachmentContainer, blobKey);

        var attachment = document.AddAttachment(GuidGenerator.Create(), blobKey, mimeType, buffer.Length, sha256);
        await _documents.UpdateAsync(document, autoSave: true);

        return ToDto(attachment);
    }

    public async Task<IRemoteStreamContent> GetAttachmentContentAsync(Guid id, Guid attachmentId)
    {
        var document = await GetOwnedAsync(id);   // ownership gate covers the child too
        var attachment = document.Attachments.FirstOrDefault(a => a.Id == attachmentId)
                         ?? throw new EntityNotFoundException(typeof(Attachment), attachmentId);

        var stream = await _fileStore.GetAsync(DocumentConsts.AttachmentContainer, attachment.BlobKey);
        return new RemoteStreamContent(stream, attachment.BlobKey, attachment.MimeType);
    }

    [Authorize(DocumentsPermissions.Documents.Update)]
    public async Task DeleteAttachmentAsync(Guid id, Guid attachmentId)
    {
        var document = await GetOwnedAsync(id);
        var blobKey = document.RemoveAttachment(attachmentId);   // returns the key for exactly this

        // The 1.5 promise kept: the file dies only AFTER the row deletion commits, so a
        // rolled-back transaction never loses bytes (OnCompleted = post-commit hook).
        UnitOfWorkManager.Current!.OnCompleted(async () =>
            await _fileStore.DeleteAsync(DocumentConsts.AttachmentContainer, blobKey));

        await _documents.UpdateAsync(document, autoSave: true);
    }

    private static AttachmentDto ToDto(Attachment a) => new()
    {
        Id = a.Id,
        MimeType = a.MimeType,
        SizeBytes = a.SizeBytes,
        CreationTime = a.CreationTime
    };

    /// <summary>Not-found (404), not forbidden (403): a 403 would confirm the id exists for someone else.</summary>
    private async Task<Document> GetOwnedAsync(Guid id)
    {
        var document = await _documents.GetAsync(id);
        if (document.OwnerUserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Document), id);
        }

        return document;
    }

    // Clock (IClock), never DateTime.Now: tests can replace it, and it is UTC-consistent.
    private DateOnly Today() => DateOnly.FromDateTime(Clock.Now);

    private static DocumentDto ToDto(Document d, DateOnly today) => new()
    {
        Id = d.Id,
        HolderId = d.HolderId,
        DocumentTypeId = d.DocumentTypeId,
        Number = d.Number,
        IssueDate = d.Validity.IssueDate,
        ExpiryDate = d.Validity.ExpiryDate,
        Status = d.Status,
        Notes = d.Notes,
        PreviousExpiryDate = d.PreviousExpiryDate,
        DaysUntilExpiry = d.Validity.DaysUntilExpiry(today),
        CreationTime = d.CreationTime,
        Attachments = d.Attachments.Select(ToDto).ToList()   // AutoInclude (1.5) loads them with the root
    };
}
