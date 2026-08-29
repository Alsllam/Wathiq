using System;
using System.Collections.Generic;
using Wathiq.Documents.Extraction;

namespace Wathiq.Documents.Extraction;

/// <summary>
/// The proposal as the review UI sees it: nullable fields (null = "the AI could not read this"),
/// warnings explaining every dropped value, and enough provenance to display "extracted by
/// qwen2.5:7b, prompt v1". Never applied without the confirm endpoint (FR-DOC-005).
/// </summary>
public class ExtractionProposalDto
{
    public Guid ExtractionResultId { get; set; }
    public Guid AttachmentId { get; set; }
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? HolderName { get; set; }
    public string? DocumentKind { get; set; }
    public decimal? Confidence { get; set; }
    public List<string> Warnings { get; set; } = [];
    public string PromptVersion { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public ExtractionOutcome Outcome { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>What the user decided to keep - possibly the proposal untouched, possibly edited.</summary>
public class ConfirmExtractionDto
{
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
