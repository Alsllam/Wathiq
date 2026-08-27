using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.EntityFrameworkCore;
using Wathiq.Documents;
using Wathiq.Documents.EntityFrameworkCore;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Documents;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class DocumentsDbContextTests : WathiqEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Should_Map_To_The_Documents_Schema_And_Know_No_Host_Entities()
    {
        var provider = GetRequiredService<IDbContextProvider<DocumentsDbContext>>();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await provider.GetDbContextAsync();

            context.Model.GetDefaultSchema().ShouldBe(DocumentsDbProperties.DbSchema);
            // ADR-001 in one assertion: the module context cannot even see Identity's tables.
            context.Model.FindEntityType(typeof(Volo.Abp.Identity.IdentityUser)).ShouldBeNull();
        });
    }
}
