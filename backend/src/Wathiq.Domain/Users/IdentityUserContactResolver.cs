using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Wathiq.Shared.Users;

namespace Wathiq.Users;

/// <summary>Host-side bridge: only the composition root may look into Identity for other modules.</summary>
public class IdentityUserContactResolver : IUserContactResolver, ITransientDependency
{
    private readonly IIdentityUserRepository _users;

    public IdentityUserContactResolver(IIdentityUserRepository users)
    {
        _users = users;
    }

    public async Task<UserContact?> FindAsync(Guid userId)
    {
        var user = await _users.FindAsync(userId);
        return user == null ? null : new UserContact(user.Email, user.Name ?? user.UserName);
    }
}
