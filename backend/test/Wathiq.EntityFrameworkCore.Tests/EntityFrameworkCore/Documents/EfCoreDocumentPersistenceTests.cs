using Wathiq.Documents;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Documents;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreDocumentPersistenceTests : DocumentPersistenceTests<WathiqEntityFrameworkCoreTestModule>
{
}
