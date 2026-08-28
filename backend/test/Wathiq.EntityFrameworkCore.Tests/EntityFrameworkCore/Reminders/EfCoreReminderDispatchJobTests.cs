using Wathiq.Reminders;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Reminders;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreReminderDispatchJobTests : ReminderDispatchJobTests<WathiqEntityFrameworkCoreTestModule>
{
}
