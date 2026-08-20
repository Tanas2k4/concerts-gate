using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Auth;

/// <summary>
/// Registration request payload for a new customer account.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// Full name of the user.
    /// </summary>
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Valid email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Account password (minimum 6 characters).
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number (optional).
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }
}
