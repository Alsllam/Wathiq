using Microsoft.Extensions.Localization;
using Wathiq.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Wathiq;

[Dependency(ReplaceServices = true)]
public class WathiqBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<WathiqResource> _localizer;

    public WathiqBrandingProvider(IStringLocalizer<WathiqResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
