using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Documents.Documents;

/// <summary>
/// Child entity of the Document aggregate: it has an Id (so it can be addressed) but no
/// repository - it is created, listed and removed only through its Document (FR-DOC-004).
/// </summary>
public class Attachment : CreationAuditedEntity<Guid>
{
    public Guid DocumentId { get; private set; }
    public string BlobKey { get; private set; } = default!;   // key in IFileStore; bytes never in SQL
    public string MimeType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public string? OcrText { get; private set; }               // filled in Phase 3
    public bool IsEncrypted { get; private set; }              // false until the Phase 8 migration
    public byte[] Sha256 { get; private set; } = default!;

    private Attachment()
    {
    }

    internal Attachment(Guid id, Guid documentId, string blobKey, string mimeType, long sizeBytes, byte[] sha256)
        : base(id)
    {
        DocumentId = documentId;
        BlobKey = Check.NotNullOrWhiteSpace(blobKey, nameof(blobKey), DocumentConsts.MaxBlobKeyLength);
        MimeType = Check.NotNullOrWhiteSpace(mimeType, nameof(mimeType), DocumentConsts.MaxMimeTypeLength);
        SizeBytes = sizeBytes;
        Sha256 = Check.NotNull(sha256, nameof(sha256));
        IsEncrypted = false;
    }

    internal void SetOcrText(string? text) => OcrText = text;
}
