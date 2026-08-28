using Wathiq.Reminders;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Reminders;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreReminderSchedulerSyncTests : ReminderSchedulerSyncTests<WathiqEntityFrameworkCoreTestModule>
{
}
