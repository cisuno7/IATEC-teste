using AgendaManager.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace AgendaManager.Infrastructure.Services;

public class TokenExtractor : ITokenExtractor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenExtractor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string? ExtractToken()
    {
        var header = _httpContextAccessor.HttpContext?
            .Request.Headers["Authorization"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(header))
            return null;

        if (!header.StartsWith("Bearer "))
            return null;

        var token = header["Bearer ".Length..].Trim();

        if (!token.Contains('.'))
            return null;

        return token;
    }
    public Guid? ExtractUserId()
    {
        var token = ExtractToken();
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
            
            if (subClaim == null || string.IsNullOrEmpty(subClaim.Value))
                return null;

            if (Guid.TryParse(subClaim.Value, out var userId))
                return userId;

            return null;
        }
        catch
        {
            return null;
        }
    }
}

