namespace Wathiq.Ai.Usage;

// Stored as tinyint (database.md). Never renumber: the values are persisted.
public enum AiUsagePurpose : byte
{
    Extraction = 0,
    Embedding = 1,
    GuideChat = 2,
    Eval = 3
}
