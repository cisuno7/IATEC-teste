using AgendaManager.Application.Interfaces;
using Supabase;
using Supabase.Gotrue;

namespace AgendaManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly Supabase.Client _supabase;

    public AuthService(Supabase.Client supabase)
    {
        _supabase = supabase ?? throw new ArgumentNullException(nameof(supabase));
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        var session = await _supabase.Auth.SignIn(email, password);

        if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
            throw new UnauthorizedAccessException("Invalid credentials");

        return new AuthResult
        {
            AccessToken = session.AccessToken,
            RefreshToken = session.RefreshToken,
            ExpiresAt = session.ExpiresAt(),
            User = MapToAuthUser(session.User)
        };
    }

    public async Task<AuthResult> SignUpAsync(string email, string password, string name)
    {
        var signUpOptions = new SignUpOptions
        {
            Data = new Dictionary<string, object>
            {
                { "name", name }
            }
        };

        var session = await _supabase.Auth.SignUp(email, password, signUpOptions);

        if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
            throw new InvalidOperationException("Registration failed");

        return new AuthResult
        {
            AccessToken = session.AccessToken,
            RefreshToken = session.RefreshToken,
            ExpiresAt = session.ExpiresAt(),
            User = MapToAuthUser(session.User)
        };
    }

    public async Task<AuthUser?> GetUserAsync(string jwt)
    {
        try
        {
            var user = await _supabase.Auth.GetUser(jwt);

            if (user == null)
                return null;

            return MapToAuthUser(user);
        }
        catch
        {
            return null;
        }
    }

    public async Task SignOutAsync()
    {
        await _supabase.Auth.SignOut();
    }

    private AuthUser MapToAuthUser(User user)
    {
        if (string.IsNullOrEmpty(user.Id))
            throw new InvalidOperationException("User ID cannot be null or empty");

        var name = ExtractUserName(user);

        return new AuthUser
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = name,
            CreatedAt = user.CreatedAt
        };
    }

    private string ExtractUserName(User user)
    {
        var nameFromMetadata = GetNameFromUserMetadata(user);
        if (!string.IsNullOrWhiteSpace(nameFromMetadata))
            return nameFromMetadata;

        var nameFromEmail = ExtractNameFromEmail(user.Email);
        if (!string.IsNullOrWhiteSpace(nameFromEmail))
            return nameFromEmail;

        return "Usuário";
    }

    private string? GetNameFromUserMetadata(User user)
    {
        if (user.UserMetadata == null)
            return null;

        if (!user.UserMetadata.ContainsKey("name"))
            return null;

        return TryExtractNameFromValue(user.UserMetadata["name"]);
    }

    private string? TryExtractNameFromValue(object? value)
    {
        if (value == null)
            return null;

        var name = value.ToString();
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return name;
    }

    private string? ExtractNameFromEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var emailParts = email.Split('@');
        if (emailParts.Length > 0 && !string.IsNullOrWhiteSpace(emailParts[0]))
            return emailParts[0];

        return null;
    }
}

