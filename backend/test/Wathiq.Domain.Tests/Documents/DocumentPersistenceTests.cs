using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Documents;
using Wathiq.Documents.Holders;
using Xunit;

namespace Wathiq.Documents;

/* Round-trips the aggregate through EF: owned type -> two columns and back, attachments
 * auto-included, both inside the module's own DbContext. Concrete class in EFCore.Tests. */
public abstract class DocumentPersistenceTests<TStartupModule> : WathiqDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRepository<Document, Guid> _documents;
    private readonly IRepository<DocumentType, Guid> _types;
    private readonly HolderManager _holderManager;

    protected DocumentPersistenceTests()
    {
        _documents = GetRequiredService<IRepository<Document, Guid>>();
        _types = GetRequiredService<IRepository<DocumentType, Guid>>();
        _holderManager = GetRequiredService<HolderManager>();
    }

    [Fact]
    public async Task Should_Persist_Validity_And_Attachments_With_The_Root()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var holder = await _holderManager.EnsureSelfHolderAsync(userId, "Amina");
            var passport = await _types.GetAsync(t => t.Code == "PASSPORT");

            var doc = new Document(id, userId, holder.Id, passport.Id,
                new ValidityPeriod(new DateOnly(2020, 5, 1), new DateOnly(2030, 5, 1)), "P-778");
            doc.AddAttachment(Guid.NewGuid(), "abc.jpg", "image/jpeg", 100, new byte[32]);

            await _documents.InsertAsync(doc);
        });

        var loaded = await WithUnitOfWorkAsync(() => _documents.GetAsync(id));

        loaded.Validity.ShouldBe(new ValidityPeriod(new DateOnly(2020, 5, 1), new DateOnly(2030, 5, 1)));
        loaded.Attachments.Single().BlobKey.ShouldBe("abc.jpg"); // AutoInclude, no explicit Include
        loaded.Status.ShouldBe(DocumentStatus.Active);
    }
}
