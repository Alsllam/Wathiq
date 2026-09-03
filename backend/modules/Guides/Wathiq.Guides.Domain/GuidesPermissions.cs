namespace Wathiq.Guides;

/// <summary>
/// Permission names for the Guides module. Deliberately asymmetric (the inverse of Documents):
/// READING guides is permissionless - published renewal guides are public content, the product's
/// community half. Only AUTHORING (create/edit/publish versions) is guarded, and only admins
/// hold it. So this module defines exactly one grantable name.
/// </summary>
public static class GuidesPermissions
{
    public const string GroupName = "WathiqGuides";

    public static class Guides
    {
        // One name for the whole authoring surface; children (Publish, Delete...) can split
        // out later WITHOUT a migration - permissions are definitions, not schema.
        public const string Manage = GroupName + ".Manage";
    }

    // Kept for symmetry with DocumentsPermissions.All - but NOT fed to the default-role seeder:
    // ordinary users never get Manage, and admin gets everything automatically at permission seed.
    public static readonly string[] All =
    [
        Guides.Manage
    ];
}
