using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Holders;
using Xunit;

namespace Wathiq.Documents;

/* Needs a database (Sqlite in-memory via the EF test module), so the concrete class lives in
 * Wathiq.EntityFrameworkCore.Tests - same split as the file-store tests in step 1.2. */
public abstract class DocumentsSeedAndHolderManagerTests<TStartupModule> : WathiqDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRepository<DocumentType, Guid> _documentTypes;
    private readonly HolderManager _holderManager;

    protected DocumentsSeedAndHolderManagerTests()
    {
        _documentTypes = GetRequiredService<IRepository<DocumentType, Guid>>();
        _holderManager = GetRequiredService<HolderManager>();
    }

    [Fact]
    public async Task Seed_Should_Create_The_Catalogue_Once()
    {
        // WathiqTestBaseModule already ran IDataSeeder at startup; the contributor is idempotent by Code.
        (await _documentTypes.GetCountAsync()).ShouldBe(8);
        (await _documentTypes.FindAsync(t => t.Code == "PASSPORT"))!.NameAr.ShouldBe("جواز السفر");
    }

    [Fact]
    public async Task EnsureSelfHolder_Should_Return_The_Same_Holder_Twice()
    {
        var userId = Guid.NewGuid();

        var first = await WithUnitOfWorkAsync(() => _holderManager.EnsureSelfHolderAsync(userId, "Amina"));
        var second = await WithUnitOfWorkAsync(() => _holderManager.EnsureSelfHolderAsync(userId, "Amina"));

        second.Id.ShouldBe(first.Id);
        first.IsSelf.ShouldBeTrue();
    }
}
