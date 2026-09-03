namespace Wathiq.Guides;

public static class GuidesDbProperties
{
    public const string DbSchema = "guides";               // database.md DB1: schema-per-module
    public const string ConnectionStringName = "Guides";   // falls back to "Default" (see DocumentsDbProperties)
}
