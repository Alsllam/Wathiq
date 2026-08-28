namespace Wathiq.Reminders;

/// <summary>Permission names; in Domain for the same reason as DocumentsPermissions (1.6).</summary>
public static class RemindersPermissions
{
    public const string GroupName = "WathiqReminders";

    public static class Rule
    {
        public const string Default = GroupName + ".Rule";
        public const string Update = Default + ".Update";
    }

    public static class Reminders
    {
        public const string Default = GroupName + ".Reminders";
    }

    public static readonly string[] All =
    [
        Rule.Default, Rule.Update,
        Reminders.Default
    ];
}
