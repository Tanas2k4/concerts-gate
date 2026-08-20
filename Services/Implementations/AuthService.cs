using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Common.Exceptions;
using concerts_gate.server.DTOs.Auth;
using concerts_gate.server.Entities;
using concerts_gate.server.Services.Interfaces;

namespace concerts_gate.server.Services.Implementations;

/// <summary>
/// Implementation of user authentication and JWT token generation service.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthService"/>.
    /// </summary>
    /// <param name="userManager">ASP.NET Identity user manager.</param>
    /// <param name="configuration">Application configuration.</param>
    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (existingUser != null)
        {
            throw new BadRequestException("This email address is already registered in the system.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = UserRole.Customer,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new BadRequestException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, AppConstants.Roles.Customer);

        var (token, expiresAt) = GenerateJwtToken(user, AppConstants.Roles.Customer);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = user.Role,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new BadRequestException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new BadRequestException("Your account is currently locked or suspended.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? user.Role.ToString();

        var (token, expiresAt) = GenerateJwtToken(user, primaryRole);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = user.Role,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    /// <inheritdoc />
    public async Task<UserProfileDto> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User account not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Helper method to generate JSON Web Tokens (JWT).
    /// </summary>
    /// <param name="user">User entity.</param>
    /// <param name="role">Role name.</param>
    /// <returns>A tuple of Token string and expiration timestamp.</returns>
    private (string Token, DateTime ExpiresAt) GenerateJwtToken(ApplicationUser user, string role)
    {
        var jwtSecret = _configuration["JwtSettings:Secret"] ?? "DefaultSuperSecretKeyForConcertsGate2026!#$*&^%";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "ConcertsGateServer";
        var audience = _configuration["JwtSettings:Audience"] ?? "ConcertsGateClient";
        var durationMinutes = int.TryParse(_configuration["JwtSettings:DurationMinutes"], out var d) ? d : 1440; // 1 day

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(durationMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, role),
            new Claim("RoleEnum", ((int)user.Role).ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return (tokenHandler.WriteToken(tokenDescriptor), expiresAt);
    }
}
