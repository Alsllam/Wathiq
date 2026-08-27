using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Wathiq.Documents.Localization;

namespace Wathiq.Documents.Permissions;

/// <summary>
/// Registers the module's permissions. ABP collects every PermissionDefinitionProvider at startup;
/// the names become checkable policies ([Authorize(...)]) and rows the admin UI can grant.
/// </summary>
public class DocumentsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(DocumentsPermissions.GroupName, L("Permission:Documents"));

        group.AddPermission(DocumentsPermissions.DocumentTypes.Default, L("Permission:DocumentTypes"));

        // Children show indented under the parent in the permission-management UI, but each name
        // is still an independent grant - a child is NOT implied by its parent.
        var holders = group.AddPermission(DocumentsPermissions.Holders.Default, L("Permission:Holders"));
        holders.AddChild(DocumentsPermissions.Holders.Create, L("Permission:Holders.Create"));
        holders.AddChild(DocumentsPermissions.Holders.Update, L("Permission:Holders.Update"));
        holders.AddChild(DocumentsPermissions.Holders.Delete, L("Permission:Holders.Delete"));

        var documents = group.AddPermission(DocumentsPermissions.Documents.Default, L("Permission:Documents.Manage"));
        documents.AddChild(DocumentsPermissions.Documents.Create, L("Permission:Documents.Create"));
        documents.AddChild(DocumentsPermissions.Documents.Update, L("Permission:Documents.Update"));
        documents.AddChild(DocumentsPermissions.Documents.Delete, L("Permission:Documents.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<WathiqDocumentsResource>(name);
    }
}
