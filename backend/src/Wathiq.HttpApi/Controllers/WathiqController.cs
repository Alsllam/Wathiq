using Wathiq.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Wathiq.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class WathiqController : AbpControllerBase
{
    protected WathiqController()
    {
        LocalizationResource = typeof(WathiqResource);
    }
}
