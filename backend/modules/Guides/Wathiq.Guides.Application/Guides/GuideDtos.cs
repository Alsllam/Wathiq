using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wathiq.Guides.Guides;

/// <summary>List row for the public guide catalogue - titles only, reader picks by language client-side.</summary>
public class GuideDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string TitleAr { get; set; } = default!;
    public string TitleEn { get; set; } = default!;
}

/// <summary>A guide opened for reading: the served (published, immutable) content of one language.</summary>
public class GuideDetailDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = default!;
    public string TitleAr { get; set; } = default!;
    public string TitleEn { get; set; } = default!;
    public GuideVersionDto Version { get; set; } = default!;
}

public class GuideVersionDto
{
    public Guid Id { get; set; }
    public Guid GuideId { get; set; }
    public int VersionNo { get; set; }
    public string Language { get; set; } = default!;
    public string BodyMarkdown { get; set; } = default!;
    public string? RequiredDocuments { get; set; }
    public string? Fees { get; set; }
    public string? Location { get; set; }
    /// <summary>Vision R2: readers always see how fresh the content is.</summary>
    public DateOnly LastVerifiedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<string> Steps { get; set; } = [];
}

public class CreateGuideDto
{
    [Required]
    [StringLength(GuideConsts.MaxSlugLength)]
    [RegularExpression("^[a-z0-9][a-z0-9-]*$")]   // URL-safe by contract, not by cleanup
    public string Slug { get; set; } = default!;

    [Required]
    [StringLength(GuideConsts.MaxTitleLength)]
    public string TitleAr { get; set; } = default!;

    [Required]
    [StringLength(GuideConsts.MaxTitleLength)]
    public string TitleEn { get; set; } = default!;
}

public class CreateGuideVersionDto
{
    [Required]
    public Guid GuideId { get; set; }

    [Required]
    [StringLength(GuideConsts.LanguageLength, MinimumLength = GuideConsts.LanguageLength)]
    public string Language { get; set; } = default!;

    [Required]
    public string BodyMarkdown { get; set; } = default!;

    /// <summary>Mandatory on purpose - unverified content must not be authorable, let alone publishable.</summary>
    [Required]
    public DateOnly LastVerifiedAt { get; set; }

    [StringLength(GuideConsts.MaxRequiredDocumentsLength)]
    public string? RequiredDocuments { get; set; }

    [StringLength(GuideConsts.MaxFeesLength)]
    public string? Fees { get; set; }

    [StringLength(GuideConsts.MaxLocationLength)]
    public string? Location { get; set; }

    public string[] Steps { get; set; } = [];
}

public class UpdateGuideVersionDto
{
    [Required]
    public string BodyMarkdown { get; set; } = default!;

    [Required]
    public DateOnly LastVerifiedAt { get; set; }

    [StringLength(GuideConsts.MaxRequiredDocumentsLength)]
    public string? RequiredDocuments { get; set; }

    [StringLength(GuideConsts.MaxFeesLength)]
    public string? Fees { get; set; }

    [StringLength(GuideConsts.MaxLocationLength)]
    public string? Location { get; set; }

    public string[] Steps { get; set; } = [];
}
