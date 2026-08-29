using System;

namespace Wathiq.Documents.Events;

/// <summary>
/// Raised by the Document aggregate when an attachment is added. Unlike DocumentExpiryChangedEto
/// this contract stays INSIDE the module: only Documents' own OCR pipeline listens, and Shared is
/// reserved for contracts that cross a module boundary - location follows the audience.
/// </summary>
[Serializable]
public class AttachmentUploadedEto
{
    public Guid DocumentId { get; set; }
    public Guid AttachmentId { get; set; }
}
