using System;
using System.Collections.Generic;

namespace Wathiq.Shared.Extraction;

/// <summary>
/// What the AI *suggests* about a document — never what the system believes. Every field is
/// nullable because every field must survive validation to appear at all (FR-AI-003): a value
/// the parsers reject becomes null plus a line in <see cref="Warnings"/>. The user confirms
/// before anything is written to a Document (FR-DOC-005).
/// </summary>
public class DocumentDataProposal
{
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? HolderName { get; set; }
    /// <summary>Free-text kind the model saw ("passport", "رخصة قيادة") - a hint for the UI, never a type id.</summary>
    public string? DocumentKind { get; set; }
    /// <summary>Model's overall 0-1 self-estimate; null when absent or out of range.</summary>
    public decimal? Confidence { get; set; }

    /// <summary>Human-readable reasons fields were dropped - shown in the review UI (FR-DOC-005).</summary>
    public List<string> Warnings { get; set; } = [];

    // Provenance, so the caller can persist an ExtractionResult without re-asking the Ai module.
    public string RawJson { get; set; } = string.Empty;
    public string PromptVersion { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int DurationMs { get; set; }
}
