using concerts_gate.server.Common.Enums;

namespace concerts_gate.server.DTOs.Auth;

/// <summary>
/// Authentication response payload containing user info and a JWT token.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// User identifier (GUID).
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User role (Customer, Operator, Admin).
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// JSON Web Token (JWT) to attach to HTTP Authorization Header (Bearer token).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration timestamp (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
