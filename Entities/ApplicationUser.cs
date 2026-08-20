using Microsoft.AspNetCore.Identity;
using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.Entities;

/// <summary>
/// User entity extending <see cref="IdentityUser{TKey}"/> with a primary key of type <see cref="Guid"/>.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Role of the user within the system (Customer, Operator, Admin).
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Customer;

    /// <summary>
    /// Account creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Flag indicating whether the account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Collection of booking orders placed by this user.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>
    /// History of voucher redemptions by this user.
    /// </summary>
    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
