namespace concerts_gate.server.Common.Enums;

/// <summary>
/// User roles within the system (Customer, Operator, Admin).
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular customer purchasing tickets.
    /// </summary>
    Customer = 0,

    /// <summary>
    /// Operations staff managing concerts, monitoring bookings, and handling issues.
    /// </summary>
    Operator = 1,

    /// <summary>
    /// System Administrator with full administrative privileges.
    /// </summary>
    Admin = 2
}
