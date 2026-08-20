using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using concerts_gate.server.Common.Models;
using concerts_gate.server.DTOs.Auth;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Controllers;

/// <summary>
/// Provides account authentication APIs including registration, login, and user profile retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">Authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new customer account in the Concerts Gate system.
    /// </summary>
    /// <param name="request">Registration details (Email, Password, Full Name, Phone Number).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Registration successful, returns JWT token.</response>
    /// <response code="400">Invalid payload or email already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Account registered successfully!"));
    }

    /// <summary>
    /// Authenticates credentials and returns a JWT Bearer token.
    /// </summary>
    /// <param name="request">Login credentials (email and password).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Login successful, returns JWT token.</response>
    /// <response code="400">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful!"));
    }

    /// <summary>
    /// Retrieves profile details for the currently authenticated user based on JWT claims.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns user profile information.</response>
    /// <response code="401">Unauthorized or invalid token.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Unable to identify user."));
        }

        var profile = await _authService.GetCurrentUserProfileAsync(userId, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }
}
