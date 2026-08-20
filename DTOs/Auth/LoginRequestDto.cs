using System.ComponentModel.DataAnnotations;

namespace concerts_gate.server.DTOs.Auth;

/// <summary>
/// Login request payload using email and password.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Account email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Account password.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
