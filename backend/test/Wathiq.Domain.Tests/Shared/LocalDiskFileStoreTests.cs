using System.IO;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Wathiq.Shared.Files;
using Xunit;

namespace Wathiq.Shared;

/* Concrete run wiring lives in Wathiq.EntityFrameworkCore.Tests, same convention as
 * Wathiq.Samples.SampleDomainTests — the full ABP application (incl. this module's DI
 * registrations) only spins up where a test database provider is configured. */
public abstract class LocalDiskFileStoreTests<TStartupModule> : WathiqDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IFileStore _fileStore;

    protected LocalDiskFileStoreTests()
    {
        _fileStore = GetRequiredService<IFileStore>();
    }

    [Fact]
    public async Task Should_Round_Trip_A_Saved_File()
    {
        var content = Encoding.UTF8.GetBytes("hello wathiq");

        string blobKey;
        await using (var input = new MemoryStream(content))
        {
            blobKey = await _fileStore.SaveAsync("tests", "note.txt", input);
        }

        blobKey.ShouldEndWith(".txt");

        await using (var saved = await _fileStore.GetAsync("tests", blobKey))
        using (var reader = new StreamReader(saved))
        {
            (await reader.ReadToEndAsync()).ShouldBe("hello wathiq");
        }

        (await _fileStore.DeleteAsync("tests", blobKey)).ShouldBeTrue();
        (await _fileStore.DeleteAsync("tests", blobKey)).ShouldBeFalse(); // already gone
    }

    [Fact]
    public async Task Should_Reject_A_File_Larger_Than_The_Configured_Limit()
    {
        var oversized = new byte[21 * 1024 * 1024]; // default FileStoreOptions.MaxSizeBytes is 20 MB
        await using var input = new MemoryStream(oversized);

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _fileStore.SaveAsync("tests", "big.bin", input));

        exception.Code.ShouldBe(WathiqSharedErrorCodes.FileTooLarge);
    }

    [Fact]
    public async Task Should_Throw_A_Localizable_Exception_When_Getting_A_Missing_File()
    {
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _fileStore.GetAsync("tests", "does-not-exist.txt"));

        exception.Code.ShouldBe(WathiqSharedErrorCodes.FileNotFound);
    }
}
