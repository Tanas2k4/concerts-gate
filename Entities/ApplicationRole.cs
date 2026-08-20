using Microsoft.AspNetCore.Identity;

namespace concerts_gate.server.Entities;

/// <summary>
/// Application role entity extending <see cref="IdentityRole{TKey}"/> with a primary key of type <see cref="Guid"/>.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Initializes a new default role instance.
    /// </summary>
    public ApplicationRole() : base() { }

    /// <summary>
    /// Initializes a role instance with a specific role name.
    /// </summary>
    /// <param name="roleName">The role name (Admin, Operator, Customer).</param>
    public ApplicationRole(string roleName) : base(roleName) { }
}
