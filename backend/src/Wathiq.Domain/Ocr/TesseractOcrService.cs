using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Wathiq.Shared.Ocr;

namespace Wathiq.Ocr;

/// <summary>
/// The Tesseract adapter: shells out to the `tesseract` CLI instead of binding native libraries -
/// one process per image is plenty at this scale, and apt/winget owns the install (no .so/.dll
/// hunting in NuGet). Privacy rail C1 holds by construction: a local child process, no network.
/// </summary>
public class TesseractOcrService : IOcrService, ITransientDependency
{
    private readonly OcrOptions _options;

    public TesseractOcrService(IOptions<OcrOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string?> ExtractTextAsync(Stream content, string mimeType, CancellationToken cancellationToken = default)
    {
        // Tesseract reads images, not PDFs; null = "engine can't read this", the caller records it.
        if (!mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // The CLI wants a file; the blob store hands us a stream. Extension is irrelevant -
        // leptonica sniffs the format from the bytes.
        var tempPath = Path.GetTempFileName();
        try
        {
            await using (var tempFile = File.Create(tempPath))
            {
                await content.CopyToAsync(tempFile, cancellationToken);
            }

            // "stdout" as the output name = print text instead of writing <name>.txt next to it.
            var startInfo = new ProcessStartInfo
            {
                FileName = _options.TesseractPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            startInfo.ArgumentList.Add(tempPath);
            startInfo.ArgumentList.Add("stdout");
            startInfo.ArgumentList.Add("-l");
            startInfo.ArgumentList.Add(_options.Languages);

            using var process = Process.Start(startInfo)
                                ?? throw new InvalidOperationException($"Failed to start '{_options.TesseractPath}'.");
            var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                // Throw, don't swallow: the background job's retry/dashboard is the right place
                // for "tesseract missing/broken" to surface, not a silent null.
                throw new InvalidOperationException(
                    $"tesseract exited with {process.ExitCode} for a {mimeType} attachment: {stderr.Trim()}");
            }

            // Empty-but-successful stays "" (processed, nothing readable) - distinct from null.
            return stdout.Trim();
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
