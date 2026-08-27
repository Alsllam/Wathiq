namespace Wathiq.Documents;

/// <summary>
/// Permission names for the Documents module. A full ABP module template would put these in an
/// Application.Contracts project; in our merged four-project layout the Domain project is the
/// lowest layer every consumer (the Application definition provider, the host role seeder)
/// already references, so the names live here.
/// </summary>
public static class DocumentsPermissions
{
    public const string GroupName = "WathiqDocuments";

    public static class DocumentTypes
    {
        public const string Default = GroupName + ".DocumentTypes";
    }

    public static class Holders
    {
        public const string Default = GroupName + ".Holders";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Documents
    {
        public const string Default = GroupName + ".Documents";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    // Single source for "grant everything of this module" (used by the host's role seeder).
    public static readonly string[] All =
    [
        DocumentTypes.Default,
        Holders.Default, Holders.Create, Holders.Update, Holders.Delete,
        Documents.Default, Documents.Create, Documents.Update, Documents.Delete
    ];
}
