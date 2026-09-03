using System;

namespace Wathiq.Guides.Events;

/// <summary>Raised by GuideVersion.Publish; the Application handler enqueues the embed job post-commit.</summary>
public class GuideVersionPublishedEto
{
    public Guid GuideVersionId { get; set; }
}
