using AgendaManager.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AgendaManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthService(string jwtSecret, string jwtIssuer = "AgendaManager", string jwtAudience = "AgendaManagerUsers")
    {
        _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
    }

    public Task<AuthResult> SignInAsync(string email, string password)
    {
        throw new InvalidOperationException("Use AuthenticateUserAsync for authentication");
    }

    public Task<AuthResult> SignUpAsync(string email, string password, string name)
    {
        throw new InvalidOperationException("Use CreateUserAsync for registration");
    }

    public async Task<AuthUser?> GetUserAsync(string jwt)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtIssuer,
                ValidAudience = _jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out _);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.FindFirst("name")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return null;

            return new AuthUser
            {
                Id = userId,
                Email = email,
                Name = name ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    public Task SignOutAsync()
    {
        return Task.CompletedTask;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string GenerateJwtToken(string userId, string email, string name)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("name", name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

