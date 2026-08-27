namespace Wathiq.Reminders;

public static class RemindersDbProperties
{
    // Every table of this module lives in its own SQL schema (database.md DB1).
    public const string DbSchema = "reminders";

    // Same database today; a config change could point this module elsewhere (see DocumentsDbProperties).
    public const string ConnectionStringName = "Reminders";
}
