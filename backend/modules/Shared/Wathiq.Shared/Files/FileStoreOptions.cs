using System;

namespace Wathiq.Shared.Files;

public class FileStoreOptions
{
    /// <summary>Relative to <see cref="AppContext.BaseDirectory"/> unless rooted. Bound from configuration section "FileStore".</summary>
    public string RootPath { get; set; } = "App_Data/wathiq-files";

    public long MaxSizeBytes { get; set; } = 20 * 1024 * 1024; // 20 MB — a phone photo of a document comfortably fits
}
