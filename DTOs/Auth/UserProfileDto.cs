namespace concerts_gate.server.DTOs.Auth;

/// <summary>
/// Profile information for the currently authenticated user.
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User role name.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Account creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
