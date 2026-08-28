using Volo.Abp.Application.Services;
using Wathiq.Reminders.Localization;

namespace Wathiq.Reminders;

public abstract class RemindersAppServiceBase : ApplicationService
{
    protected RemindersAppServiceBase()
    {
        LocalizationResource = typeof(WathiqRemindersResource);
    }
}
