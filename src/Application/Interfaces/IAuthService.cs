namespace AgendaManager.Application.Interfaces;

public interface IAuthService
{
    Task<AuthUser?> GetUserAsync(string jwt);
    Task SignOutAsync();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateJwtToken(string userId, string email, string name);
}

public class AuthResult
{
    public required string AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public required AuthUser User { get; set; }
}

public class AuthUser
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required DateTime CreatedAt { get; set; }
}

