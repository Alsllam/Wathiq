using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Wathiq.Shared.Files;

/// <summary>
/// Dev/single-VPS implementation of <see cref="IFileStore"/>: plain files on local disk.
/// Bytes are written and read as-is — encryption at rest is Phase 8 (NFR-SEC-001); the two
/// spots below marked "TODO(P8)" are exactly where an IEncryptor would wrap the stream.
/// </summary>
public class LocalDiskFileStore : IFileStore, ITransientDependency
{
    private const int CopyBufferSize = 81920;

    private readonly FileStoreOptions _options;

    public LocalDiskFileStore(IOptions<FileStoreOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(string containerName, string suggestedFileName, Stream content, CancellationToken cancellationToken = default)
    {
        EnsureSafeSegment(containerName, nameof(containerName));

        var containerPath = Directory.CreateDirectory(Path.Combine(ResolveRoot(), containerName)).FullName;
        var blobKey = Guid.NewGuid().ToString("N") + Path.GetExtension(suggestedFileName);
        var targetPath = Path.Combine(containerPath, blobKey);

        // Copy in chunks rather than trusting content.Length: an upload stream is not always
        // seekable, and this lets us reject an oversized file without buffering it all in memory.
        var totalBytes = 0L;
        var buffer = new byte[CopyBufferSize];
        await using var target = File.Create(targetPath);
        // TODO(P8): wrap `target` with an IEncryptor CryptoStream before writing (NFR-SEC-001).
        int read;
        while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
        {
            totalBytes += read;
            if (totalBytes > _options.MaxSizeBytes)
            {
                await target.DisposeAsync();
                File.Delete(targetPath);
                throw new BusinessException(WathiqSharedErrorCodes.FileTooLarge)
                    .WithData("MaxSizeBytes", _options.MaxSizeBytes);
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        return blobKey;
    }

    public Task<Stream> GetAsync(string containerName, string blobKey, CancellationToken cancellationToken = default)
    {
        EnsureSafeSegment(containerName, nameof(containerName));
        EnsureSafeSegment(blobKey, nameof(blobKey));

        var path = Path.Combine(ResolveRoot(), containerName, blobKey);
        if (!File.Exists(path))
        {
            throw new BusinessException(WathiqSharedErrorCodes.FileNotFound);
        }

        // TODO(P8): wrap the returned stream with an IEncryptor CryptoStream before returning (NFR-SEC-001).
        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    public Task<bool> DeleteAsync(string containerName, string blobKey, CancellationToken cancellationToken = default)
    {
        EnsureSafeSegment(containerName, nameof(containerName));
        EnsureSafeSegment(blobKey, nameof(blobKey));

        var path = Path.Combine(ResolveRoot(), containerName, blobKey);
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);
        return Task.FromResult(true);
    }

    private string ResolveRoot()
    {
        return Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(AppContext.BaseDirectory, _options.RootPath);
    }

    // Rejects path-traversal segments (e.g. "..", "sub/dir") — every caller-supplied name must be a bare file/folder name.
    private static void EnsureSafeSegment(string segment, string paramName)
    {
        if (string.IsNullOrWhiteSpace(segment) || segment != Path.GetFileName(segment))
        {
            throw new ArgumentException($"'{segment}' is not a valid single path segment.", paramName);
        }
    }
}
