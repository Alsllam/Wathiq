using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Wathiq.Reminders.Localization;

namespace Wathiq.Reminders.Permissions;

public class RemindersPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(RemindersPermissions.GroupName, L("Permission:Reminders"));

        var rule = group.AddPermission(RemindersPermissions.Rule.Default, L("Permission:Rule"));
        rule.AddChild(RemindersPermissions.Rule.Update, L("Permission:Rule.Update"));

        group.AddPermission(RemindersPermissions.Reminders.Default, L("Permission:Reminders.List"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<WathiqRemindersResource>(name);
    }
}
