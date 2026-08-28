using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // CheckErrors() lives in this namespace (ABP extension)
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Wathiq.Documents;

namespace Wathiq.Data;

/// <summary>
/// Wathiq is a consumer app: every registered person must be able to manage their own documents
/// without an admin granting anything. This seeds a "user" role marked IsDefault (ABP Identity
/// assigns default roles to each new registration) and grants it the Documents permissions.
/// It lives in the host, not the module: only the host may touch Identity AND Documents at once.
/// </summary>
public class WathiqUserRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string UserRoleName = "user";

    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IGuidGenerator _guidGenerator;

    public WathiqUserRoleDataSeedContributor(
        IdentityRoleManager roleManager,
        IPermissionDataSeeder permissionDataSeeder,
        IGuidGenerator guidGenerator)
    {
        _roleManager = roleManager;
        _permissionDataSeeder = permissionDataSeeder;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _roleManager.FindByNameAsync(UserRoleName) == null)
        {
            var role = new IdentityRole(_guidGenerator.Create(), UserRoleName, context.TenantId)
            {
                IsDefault = true, // this flag is what attaches the role to every new registration
                IsPublic = true
            };
            (await _roleManager.CreateAsync(role)).CheckErrors();
        }

        // Grants are rows in AbpPermissionGrants keyed by (provider "R", role name, permission name);
        // the seeder is idempotent, so re-running the migrator only adds what is missing.
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            UserRoleName,
            [.. DocumentsPermissions.All, .. Wathiq.Reminders.RemindersPermissions.All],
            context.TenantId);
    }
}
