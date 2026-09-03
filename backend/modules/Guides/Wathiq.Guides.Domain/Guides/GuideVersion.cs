using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Guids;

namespace Wathiq.Guides.Guides;

/// <summary>
/// One authored snapshot of a guide's content (database.md E-GuideVersion). The module's core
/// rule lives here: <b>a published version is immutable</b> - 5.3's chunks and 5.5's citations
/// reference a GuideVersionId, so editing published content would make past answers describe
/// text that no longer exists (the ExtractionResult append-only idea applied to content).
/// Re-authoring = a new draft version.
/// </summary>
public class GuideVersion : FullAuditedAggregateRoot<Guid>
{
    public Guid GuideId { get; private set; }
    public int VersionNo { get; private set; }
    public string Language { get; private set; } = default!;
    public string BodyMarkdown { get; private set; } = default!;
    public string? RequiredDocuments { get; private set; }
    public string? Fees { get; private set; }
    public string? Location { get; private set; }
    /// <summary>Mandatory (Vision R2): every answer shows how fresh its source is.</summary>
    public DateOnly LastVerifiedAt { get; private set; }
    /// <summary>Null = draft. Set once; never cleared (unpublishing happens by publishing a successor).</summary>
    public DateTime? PublishedAt { get; private set; }

    public List<GuideStep> Steps { get; private set; } = [];

    public bool IsPublished => PublishedAt is not null;

    private GuideVersion()
    {
    }

    public GuideVersion(
        Guid id, Guid guideId, int versionNo, string language, string bodyMarkdown,
        DateOnly lastVerifiedAt, string? requiredDocuments = null, string? fees = null, string? location = null)
        : base(id)
    {
        GuideId = guideId;
        VersionNo = versionNo;
        Language = ValidateLanguage(language);
        BodyMarkdown = Check.NotNullOrWhiteSpace(bodyMarkdown, nameof(bodyMarkdown));
        LastVerifiedAt = lastVerifiedAt;
        RequiredDocuments = Check.Length(requiredDocuments, nameof(requiredDocuments), GuideConsts.MaxRequiredDocumentsLength);
        Fees = Check.Length(fees, nameof(fees), GuideConsts.MaxFeesLength);
        Location = Check.Length(location, nameof(location), GuideConsts.MaxLocationLength);
    }

    public GuideVersion UpdateDraft(
        string bodyMarkdown, DateOnly lastVerifiedAt,
        string? requiredDocuments, string? fees, string? location)
    {
        EnsureDraft();
        BodyMarkdown = Check.NotNullOrWhiteSpace(bodyMarkdown, nameof(bodyMarkdown));
        LastVerifiedAt = lastVerifiedAt;
        RequiredDocuments = Check.Length(requiredDocuments, nameof(requiredDocuments), GuideConsts.MaxRequiredDocumentsLength);
        Fees = Check.Length(fees, nameof(fees), GuideConsts.MaxFeesLength);
        Location = Check.Length(location, nameof(location), GuideConsts.MaxLocationLength);
        return this;
    }

    /// <summary>Replaces the whole ordered list - steps are content, versioned as one unit with the body.</summary>
    public GuideVersion ReplaceSteps(IEnumerable<string> stepTexts, IGuidGenerator guidGenerator)
    {
        EnsureDraft();
        Steps.Clear();
        var no = 0;
        foreach (var text in stepTexts)
        {
            no++;
            Steps.Add(new GuideStep(guidGenerator.Create(), Id, no, text));
        }
        return this;
    }

    public GuideVersion Publish(DateTime now)
    {
        if (IsPublished)
        {
            // Publish is one-shot: PublishedAt is a historical fact, not a toggle.
            throw new BusinessException(GuidesErrorCodes.VersionAlreadyPublished)
                .WithData("VersionNo", VersionNo);
        }

        PublishedAt = now;

        // Publish is the moment content becomes citable, so it is also the moment to (re)build
        // its chunks+embeddings. AddLocalEvent publishes at SaveChanges, inside this UoW (1.5).
        AddLocalEvent(new Events.GuideVersionPublishedEto { GuideVersionId = Id });
        return this;
    }

    private void EnsureDraft()
    {
        if (IsPublished)
        {
            throw new BusinessException(GuidesErrorCodes.PublishedVersionIsImmutable)
                .WithData("VersionNo", VersionNo);
        }
    }

    private static string ValidateLanguage(string language)
    {
        Check.NotNullOrWhiteSpace(language, nameof(language));
        if (!GuideConsts.SupportedLanguages.Contains(language))
        {
            throw new ArgumentException($"Language must be one of: {string.Join(", ", GuideConsts.SupportedLanguages)}.", nameof(language));
        }
        return language;
    }
}
