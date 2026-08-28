namespace Wathiq.Documents.Documents;

public static class DocumentConsts
{
    public const int MaxNumberLength = 64;
    public const int MaxNotesLength = 1024;
    public const int MaxBlobKeyLength = 256;
    public const int MaxMimeTypeLength = 64;
    public const string AttachmentContainer = "documents"; // IFileStore container name

    // database.md E-Attachment: what a personal document photo/scan can be. Checked BEFORE any
    // byte is stored - an allow-list, never a block-list.
    public static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "application/pdf"];
}
