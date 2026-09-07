using System;
using System.ComponentModel.DataAnnotations;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.Feedback;

public class CreateGuideFeedbackDto
{
    [Required]
    public Guid GuideVersionId { get; set; }

    [EnumDataType(typeof(GuideFeedbackKind))]   // model binding would happily cast 7 into the enum
    public GuideFeedbackKind Kind { get; set; }

    [StringLength(GuideConsts.MaxFeedbackCommentLength)]
    public string? Comment { get; set; }
}

public class GuideFeedbackDto
{
    public Guid Id { get; set; }
    public Guid GuideVersionId { get; set; }
    public string GuideSlug { get; set; } = default!;
    public GuideFeedbackKind Kind { get; set; }
    public string? Comment { get; set; }
    public bool Anonymous { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
