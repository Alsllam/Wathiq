using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Wathiq.Guides.Localization;

namespace Wathiq.Guides.Permissions;

/// <summary>
/// One group, one permission: Guides.Manage guards authoring only. There is NO read permission
/// on purpose - guide-reading endpoints (5.2+) will be [AllowAnonymous]: absence of a definition
/// here is the design, not an omission.
/// </summary>
public class GuidesPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(GuidesPermissions.GroupName, L("Permission:Guides"));

        group.AddPermission(GuidesPermissions.Guides.Manage, L("Permission:Guides.Manage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<WathiqGuidesResource>(name);
    }
}
