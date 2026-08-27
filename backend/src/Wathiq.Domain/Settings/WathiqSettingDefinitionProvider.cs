using Volo.Abp.Settings;

namespace Wathiq.Settings;

public class WathiqSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(WathiqSettings.MySetting1));
    }
}
