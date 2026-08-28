using System;
using System.Threading.Tasks;

namespace Wathiq.Shared.Users;

/// <summary>How a user can be reached. Null Email means "cannot email this user".</summary>
public record UserContact(string? Email, string DisplayName);

/// <summary>
/// Contact data lives in Identity, which business modules must not reference (ADR-001).
/// Same pattern as IFileStore: Shared owns the abstraction, the HOST implements it with
/// Identity access, modules consume it blindly.
/// </summary>
public interface IUserContactResolver
{
    Task<UserContact?> FindAsync(Guid userId);
}
