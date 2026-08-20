using concerts_gate.server.DTOs.Auth;

namespace concerts_gate.server.Services.Interfaces;

/// <summary>
/// Provides business methods for user authentication and authorization (JWT Token, Register, Login).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new customer account in the system.
    /// </summary>
    /// <param name="request">Registration info including email, password, and full name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="AuthResponseDto"/> containing JWT token and basic profile.</returns>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates user credentials and issues a JWT token.
    /// </summary>
    /// <param name="request">Login request with email and password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="AuthResponseDto"/> containing JWT token.</returns>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves user profile details by user ID claims.
    /// </summary>
    /// <param name="userId">User unique identifier (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="UserProfileDto"/> with detailed profile info.</returns>
    Task<UserProfileDto> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
