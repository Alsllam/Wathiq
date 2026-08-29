namespace Wathiq.Documents.Extraction;

/// <summary>Stored as tinyint (database.md E-ExtractionResult) - append values, never renumber.</summary>
public enum ExtractionOutcome : byte
{
    Proposed = 0,
    Accepted = 1,
    /// <summary>User changed at least one proposed field before saving - 3.8's eval signal.</summary>
    Edited = 2,
    Rejected = 3,
    /// <summary>The model call itself failed; kept as a row so failure rates are queryable.</summary>
    Failed = 4
}
