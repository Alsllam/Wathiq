namespace Wathiq.Reminders.Rules;

public static class ReminderRuleConsts
{
    public const int MaxOffsetsDaysLength = 64;   // CSV column budget (database.md)
    public const int MaxTimeZoneIdLength = 64;
    public const int MaxOffsetCount = 8;          // 64 chars / "365," gives a safe ceiling
    public const int MaxOffsetDays = 3650;        // ten years before expiry is the sanity limit
    public const string DefaultTimeZoneId = "Asia/Riyadh";
}
