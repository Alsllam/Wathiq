using Wathiq.Localization;
using Volo.Abp.Application.Services;

namespace Wathiq;

/* Inherit your application services from this class.
 */
public abstract class WathiqAppService : ApplicationService
{
    protected WathiqAppService()
    {
        LocalizationResource = typeof(WathiqResource);
    }
}
