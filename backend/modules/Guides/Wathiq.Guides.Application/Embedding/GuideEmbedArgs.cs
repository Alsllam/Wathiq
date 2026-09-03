using System;

namespace Wathiq.Guides.Embedding;

/// <summary>Job args - serialized into the queue, so: small, flat, no entities (the 3.5 rule).</summary>
public class GuideEmbedArgs
{
    public Guid GuideVersionId { get; set; }
}
