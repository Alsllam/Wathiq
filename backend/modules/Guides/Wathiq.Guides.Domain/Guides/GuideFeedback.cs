using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Guides.Guides;

/// <summary>
/// A reader's flag on a published version (database.md E-GuideFeedback). This is Vision R2's
/// other half: LastVerifiedAt tells readers how fresh content is, feedback tells the ADMIN when
/// readers disagree. Anchored to the VERSION (not the guide) so "outdated" survives re-authoring
/// as a record of which snapshot earned the complaint.
/// </summary>
public class GuideFeedback : CreationAuditedAggregateRoot<Guid>
{
    public Guid GuideVersionId { get; private set; }
    /// <summary>Null = anonymous reader - feedback must not require an account (the guides are public).</summary>
    public Guid? UserId { get; private set; }
    public GuideFeedbackKind Kind { get; private set; }
    public string? Comment { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private GuideFeedback()
    {
    }

    public GuideFeedback(Guid id, Guid guideVersionId, Guid? userId, GuideFeedbackKind kind, string? comment)
        : base(id)
    {
        GuideVersionId = guideVersionId;
        UserId = userId;
        Kind = kind;
        Comment = Check.Length(comment, nameof(comment), GuideConsts.MaxFeedbackCommentLength);
    }

    /// <summary>Idempotent: "resolved" is a state, not an event log - a double-click must not throw.</summary>
    public GuideFeedback Resolve(DateTime now)
    {
        ResolvedAt ??= now;
        return this;
    }
}
