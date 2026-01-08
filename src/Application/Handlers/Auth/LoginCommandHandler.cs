using AgendaManager.Application.Commands.Auth;
using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Interfaces;
using AgendaManager.Domain.ValueObjects;
using MediatR;

namespace AgendaManager.Application.Handlers.Auth;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(IAuthService authService, IUnitOfWork unitOfWork)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(Email.Create(request.LoginData.Email));
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is not active");

        if (!_authService.VerifyPassword(request.LoginData.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = _authService.GenerateJwtToken(user.Id.ToString(), user.Email.Value, user.Name);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }
}

