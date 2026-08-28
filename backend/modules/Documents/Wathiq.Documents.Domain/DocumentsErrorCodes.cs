namespace Wathiq.Documents;

public static class DocumentsErrorCodes
{
    // Namespace "Wathiq.Documents" is mapped to WathiqDocumentsResource in the domain module.
    public const string ExpiryBeforeIssue = "Wathiq.Documents:ExpiryBeforeIssue";
    public const string HolderNotOwned = "Wathiq.Documents:HolderNotOwned";
    public const string DocumentTypeNotActive = "Wathiq.Documents:DocumentTypeNotActive";
    public const string SelfHolderIsAutomatic = "Wathiq.Documents:SelfHolderIsAutomatic";
    public const string CannotDeleteSelfHolder = "Wathiq.Documents:CannotDeleteSelfHolder";
    public const string HolderHasDocuments = "Wathiq.Documents:HolderHasDocuments";
    public const string UnsupportedFileType = "Wathiq.Documents:UnsupportedFileType";
}
