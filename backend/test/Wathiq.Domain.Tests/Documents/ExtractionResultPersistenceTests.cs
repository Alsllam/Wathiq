using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Wathiq.Documents.Extraction;
using Xunit;

namespace Wathiq.Documents;

/* The row round-trips: enum-as-byte, decimal(4,3) confidence, lengths. Note the FK: the
 * attachment must exist first, which is why this test builds a real Document. Concrete class
 * in EFCore.Tests (SQLite). */
public abstract class ExtractionResultPersistenceTests<TStartupModule> : WathiqDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRepository<Documents.Document, Guid> _documents;
    private readonly IRepository<ExtractionResult, Guid> _results;
    private readonly IRepository<DocumentTypes.DocumentType, Guid> _types;
    private readonly Holders.HolderManager _holderManager;

    protected ExtractionResultPersistenceTests()
    {
        _documents = GetRequiredService<IRepository<Documents.Document, Guid>>();
        _results = GetRequiredService<IRepository<ExtractionResult, Guid>>();
        _types = GetRequiredService<IRepository<DocumentTypes.DocumentType, Guid>>();
        _holderManager = GetRequiredService<Holders.HolderManager>();
    }

    [Fact]
    public async Task Result_Rows_Round_Trip_And_Conclude()
    {
        var userId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var resultId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            // Real holder + seeded type: Document's FKs are enforced in SQLite too - random
            // Guids bounce off them (found the honest way).
            var holder = await _holderManager.EnsureSelfHolderAsync(userId, "Amina");
            var passport = await _types.GetAsync(t => t.Code == "PASSPORT");

            var doc = new Documents.Document(
                Guid.NewGuid(), userId, holder.Id, passport.Id,
                new Documents.ValidityPeriod(null, new DateOnly(2030, 1, 1)));
            doc.AddAttachment(attachmentId, "blob-x.png", "image/png", 10, new byte[32]);
            // autoSave: the attachment row must exist before the FK-bearing result row - EF only
            // orders inserts within one SaveChanges, and these are two aggregates.
            await _documents.InsertAsync(doc, autoSave: true);

            await _results.InsertAsync(new ExtractionResult(
                resultId, attachmentId, "ollama", "qwen2.5:7b", "extract-document@v1",
                rawJson: """{"number":"P-102030"}""", confidence: 0.905m, durationMs: 2100));
        });

        var row = await _results.GetAsync(resultId);
        row.Outcome.ShouldBe(ExtractionOutcome.Proposed);
        row.Confidence.ShouldBe(0.905m);
        row.PromptVersion.ShouldBe("extract-document@v1");

        await WithUnitOfWorkAsync(async () => (await _results.GetAsync(resultId)).MarkEdited());
        (await _results.GetAsync(resultId)).Outcome.ShouldBe(ExtractionOutcome.Edited);
    }
}
