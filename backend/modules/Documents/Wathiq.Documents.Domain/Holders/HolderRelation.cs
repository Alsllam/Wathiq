namespace Wathiq.Documents.Holders;

// Stored as tinyint (database.md). Never renumber: the values are persisted.
public enum HolderRelation : byte
{
    Self = 0,
    Spouse = 1,
    Child = 2,
    Parent = 3,
    Other = 4
}
