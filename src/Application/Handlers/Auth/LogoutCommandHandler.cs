using AgendaManager.Application.Commands.Auth;
using AgendaManager.Application.Interfaces;
using MediatR;

namespace AgendaManager.Application.Handlers.Auth;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, bool>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _authService.SignOutAsync();
        return true;
    }
}

