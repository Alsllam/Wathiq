using Volo.Abp.Application.Services;
using Wathiq.Guides.Localization;

namespace Wathiq.Guides;

/// <summary>Common base so every service localizes (L[...]) from the module's own resource.</summary>
public abstract class GuidesAppServiceBase : ApplicationService
{
    protected GuidesAppServiceBase()
    {
        LocalizationResource = typeof(WathiqGuidesResource);
    }
}
