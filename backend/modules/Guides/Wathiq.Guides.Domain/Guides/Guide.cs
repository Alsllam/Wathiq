using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Guides.Guides;

/// <summary>
/// The stable identity of a renewal guide (database.md E-Guide). Content lives in
/// GuideVersions; the guide itself only knows WHICH version is currently served.
/// Slug is the public, URL-safe name - it never changes even as content is re-authored.
/// </summary>
public class Guide : FullAuditedAggregateRoot<Guid>
{
    public string Slug { get; private set; } = default!;
    public string TitleAr { get; private set; } = default!;
    public string TitleEn { get; private set; } = default!;
    /// <summary>The served version (latest publish wins). Null until first publish.</summary>
    public Guid? PublishedVersionId { get; private set; }
    public bool IsActive { get; private set; }

    private Guide()
    {
    }

    public Guide(Guid id, string slug, string titleAr, string titleEn)
        : base(id)
    {
        Slug = Check.NotNullOrWhiteSpace(slug, nameof(slug), GuideConsts.MaxSlugLength);
        TitleAr = Check.NotNullOrWhiteSpace(titleAr, nameof(titleAr), GuideConsts.MaxTitleLength);
        TitleEn = Check.NotNullOrWhiteSpace(titleEn, nameof(titleEn), GuideConsts.MaxTitleLength);
        IsActive = true;
    }

    /// <summary>Point the guide at a published version. Called by GuideManager during publish.</summary>
    public Guide SetPublishedVersion(GuideVersion version)
    {
        if (version.GuideId != Id)
        {
            throw new BusinessException(GuidesErrorCodes.VersionNotOfGuide);
        }
        if (!version.IsPublished)
        {
            // A draft must never be served: readers (and 5.5's citations) only ever see immutable content.
            throw new BusinessException(GuidesErrorCodes.CannotServeDraft);
        }

        PublishedVersionId = version.Id;
        return this;
    }

    public Guide SetActive(bool isActive)
    {
        IsActive = isActive;
        return this;
    }
}
