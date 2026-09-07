using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wathiq.Guides.Chat;

public class GuideChatRequestDto
{
    [Required]
    [StringLength(512)]   // a question, not a document - also bounds the embed call's cost
    public string Question { get; set; } = default!;
}

/// <summary>One validated citation: enough for the portal to render "source: guide X, verified Y"
/// and link to the guide - the chunk/version pair is the stable anchor (the 5.0 theme).</summary>
public class GuideChatCitationDto
{
    public Guid ChunkId { get; set; }
    public Guid GuideVersionId { get; set; }
    public string GuideSlug { get; set; } = default!;
    public string TitleAr { get; set; } = default!;
    public string TitleEn { get; set; } = default!;
    public string Snippet { get; set; } = default!;
    public DateOnly LastVerifiedAt { get; set; }
}

public class GuideChatResponseDto
{
    /// <summary>False = an honest refusal; Message says so and the portal points at the guide list.</summary>
    public bool Answered { get; set; }
    public string? Answer { get; set; }
    /// <summary>Localized refusal text when not answered.</summary>
    public string? Message { get; set; }
    public List<GuideChatCitationDto> Citations { get; set; } = [];
    /// <summary>Vision R2, answer-level: the OLDEST freshness date among cited sources - conservative on purpose.</summary>
    public DateOnly? LastVerifiedAt { get; set; }
    /// <summary>FR-AI-003 for RAG: true when the model cited labels that were never retrieved (dropped).</summary>
    public bool HallucinatedCitationsDropped { get; set; }
}
