using AgendaManager.Application.Commands.Auth;
using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using MediatR;

namespace AgendaManager.Application.Handlers.Auth;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.SignInAsync(request.LoginData.Email, request.LoginData.Password);

        var userWithMetadata = await _authService.GetUserAsync(authResult.AccessToken);
        if (userWithMetadata == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!Guid.TryParse(userWithMetadata.Id, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID");

        return new AuthResponseDto
        {
            Token = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            ExpiresAt = authResult.ExpiresAt,
            User = new UserDto
            {
                Id = userId,
                Name = userWithMetadata.Name,
                Email = userWithMetadata.Email,
                IsActive = true,
                CreatedAt = userWithMetadata.CreatedAt
            }
        };
    }
}

