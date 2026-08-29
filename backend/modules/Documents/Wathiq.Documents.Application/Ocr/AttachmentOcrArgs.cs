using System;
using Volo.Abp.BackgroundJobs;

namespace Wathiq.Documents.Ocr;

/// <summary>
/// The job's payload - serialized into the queue, so it carries IDs, never entities. The stable
/// name survives class renames the way a route survives a controller rename.
/// </summary>
[BackgroundJobName("documents-attachment-ocr")]
public class AttachmentOcrArgs
{
    public Guid DocumentId { get; set; }
    public Guid AttachmentId { get; set; }
}
