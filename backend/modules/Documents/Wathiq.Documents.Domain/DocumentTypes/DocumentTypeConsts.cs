namespace Wathiq.Documents.DocumentTypes;

// Single source for lengths: the entity guards them, the EF configuration maps them (database.md).
public static class DocumentTypeConsts
{
    public const int MaxCodeLength = 32;
    public const int MaxNameLength = 128;
}
