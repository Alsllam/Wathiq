using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wathiq.Shared.Files;

/// <summary>
/// Cross-cutting file storage abstraction (architecture.md "Shared" module). Every module that
/// needs to persist file bytes — starting with Documents.Attachment — depends on this interface,
/// never on a concrete storage technology (local disk today, S3-compatible later, per PLAN D-notes).
/// </summary>
public interface IFileStore
{
    /// <summary>Saves <paramref name="content"/> under <paramref name="containerName"/> and returns the blob key to store alongside the owning entity (e.g. Attachment.BlobKey).</summary>
    Task<string> SaveAsync(string containerName, string suggestedFileName, Stream content, CancellationToken cancellationToken = default);

    Task<Stream> GetAsync(string containerName, string blobKey, CancellationToken cancellationToken = default);

    /// <summary>Returns whether a file existed to delete.</summary>
    Task<bool> DeleteAsync(string containerName, string blobKey, CancellationToken cancellationToken = default);
}
