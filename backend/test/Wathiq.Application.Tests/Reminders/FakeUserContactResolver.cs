using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wathiq.Shared.Users;

namespace Wathiq.Reminders;

/// <summary>The host's Identity-backed resolver is out of reach here; tests script the answers.</summary>
public class FakeUserContactResolver : IUserContactResolver
{
    public Dictionary<Guid, UserContact?> Contacts { get; } = [];

    public Task<UserContact?> FindAsync(Guid userId) =>
        Task.FromResult(Contacts.GetValueOrDefault(userId, new UserContact($"{userId:N}@test.local", "Test User")));
}
