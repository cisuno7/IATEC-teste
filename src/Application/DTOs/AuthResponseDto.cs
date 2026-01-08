namespace AgendaManager.Application.DTOs;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public required UserDto User { get; set; }
}
