namespace Wathiq.Documents;

public static class DocumentsDbProperties
{
    // Every table of this module lives in its own SQL schema (database.md DB1).
    public const string DbSchema = "documents";

    // ABP resolves "Documents" from ConnectionStrings, falling back to "Default" when absent -
    // so today it is the same database, but the module could be pointed elsewhere by config alone.
    public const string ConnectionStringName = "Documents";
}
