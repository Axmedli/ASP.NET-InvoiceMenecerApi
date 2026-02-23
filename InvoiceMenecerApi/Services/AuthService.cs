using InvoiceMenecerApi.Config;
using InvoiceMenecerApi.DTOs;
using InvoiceMenecerApi.Models;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvoiceMenecerApi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InvoiceMenecerDBContext _context;
    private readonly JwtConfig _jwtConfig;

    private const string RefreshTokenType = "refresh";

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        InvoiceMenecerDBContext context,
        IOptions<JwtConfig> jwtConfig)
    {
        _userManager = userManager;
        _context = context;
        _jwtConfig = jwtConfig.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = registerRequest.Email,
            Email = registerRequest.Email,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));

            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "User");

        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        var (principial, jti) = ValidateRefreshJwtAndGetJti(refreshTokenRequest.RefreshToken);

        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.JwtId == jti);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (!storedToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token has been revoked or expired");

        var userId = principial.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _userManager.FindByIdAsync(userId!);

        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        storedToken.RevokedAt = DateTime.UtcNow;

        var newTokens = await GenerateTokenAsync(user);

        var newStoredToken = await _context
                               .RefreshTokens
                               .FirstOrDefaultAsync(rt => rt.JwtId == GetJtiFromRefreshToken(newTokens.RefreshToken));

        if (newStoredToken is not null)
            storedToken.ReplaceByJwtId = newStoredToken.JwtId;

        await _context.SaveChangesAsync();
        return newTokens;

    }
    public async Task RevokeRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        string? jti;
        try
        {
            (_, jti) = ValidateRefreshJwtAndGetJti(refreshTokenRequest.RefreshToken, validateLifeTime: false);
        }
        catch
        {
            return;
        }

        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.JwtId == jti);

        if (storedToken is null || !storedToken.IsActive) return;

        storedToken.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
    public async Task<AuthResponseDto> EditProfile(string userId, EditProfileRequest editProfileRequest)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        user.FirstName = editProfileRequest.FirstName;
        user.LastName = editProfileRequest.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto> UpdatePasswordAsync(string userId, UpdatePasswordRequest updatePasswordRequest)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        if (updatePasswordRequest.NewPassword != updatePasswordRequest.ConfirmNewPassword)
            throw new InvalidOperationException("New password and confirmation do not match");

        var result = await _userManager.ChangePasswordAsync(user, updatePasswordRequest.CurrentPassword, updatePasswordRequest.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password update failed: {errors}");
        }

        return await GenerateTokenAsync(user);
    }

    public async Task<bool> DeleteAccountAsync(string userId, DeletePasswordRequest deletePasswordRequest)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, deletePasswordRequest.CurrentPassword);

        if (!isValidPassword)
            throw new UnauthorizedAccessException("Invalid password");

        var result = await _userManager.DeleteAsync(user);
        
      
        return result.Succeeded;
    }

    private async Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };


        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var (refreshToken, refreshJwt) = await CreateRefreshTokenJwtAsync(user.Id, _jwtConfig.RefreshTokenExpirationDays);

        return new AuthResponseDto
        {
            AccessToken = tokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationMinutes),
            RefreshToken = refreshJwt,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }

    private (ClaimsPrincipal principial, string jti)
        ValidateRefreshJwtAndGetJti(string refreshToken, bool validateLifeTime = true)
    {
        var handler = new JwtSecurityTokenHandler();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.RefreshTokenSecretKey!));

        var principal = handler.ValidateToken(refreshToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifeTime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtConfig.Issuer,
            ValidAudience = _jwtConfig.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwt)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var tokenType = jwt.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;

        if (tokenType != RefreshTokenType)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        return (principal, jti);
    }

    private async Task<(RefreshToken refreshToken, string jwt)>
        CreateRefreshTokenJwtAsync(string userId, int expirationDay)
    {

        var jti = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddDays(expirationDay);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.RefreshTokenSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("token_type", RefreshTokenType)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
            );

        var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = new RefreshToken
        {
            JwtId = jti,
            UserId = userId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        return (refreshToken, jwtString);
    }

    private static string GetJtiFromRefreshToken(string refreshJwt)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(refreshJwt)) return string.Empty;

        var jwt = handler.ReadJwtToken(refreshJwt);

        return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
    }

}
