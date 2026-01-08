using AgendaManager.Application.Commands.Auth;
using AgendaManager.Application.Interfaces;
using MediatR;

namespace AgendaManager.Application.Handlers.Auth;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResult>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.SignUpAsync(
            request.RegisterData.Email,
            request.RegisterData.Password,
            request.RegisterData.Name);

        return new RegisterResult
        {
            UserId = authResult.User.Id,
            Email = authResult.User.Email
        };
    }
}

