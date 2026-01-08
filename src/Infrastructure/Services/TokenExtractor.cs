using AgendaManager.Application.Interfaces;
using Microsoft.AspNetCore.Http;

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
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return null;

        return authHeader.Substring("Bearer ".Length).Trim();
    }
}

