using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Wathiq.Guides.Guides;

/// <summary>
/// One ordered instruction of a version (database.md E-GuideStep). A child of the GuideVersion
/// aggregate - loaded with it, immutable with it, cascade-deleted with it.
/// </summary>
public class GuideStep : Entity<Guid>
{
    public Guid GuideVersionId { get; private set; }
    public int StepNo { get; private set; }
    public string Text { get; private set; } = default!;

    private GuideStep()
    {
    }

    internal GuideStep(Guid id, Guid guideVersionId, int stepNo, string text)
        : base(id)
    {
        GuideVersionId = guideVersionId;
        StepNo = stepNo;
        Text = Check.NotNullOrWhiteSpace(text, nameof(text), GuideConsts.MaxStepTextLength);
    }
}
