using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Commands.Auth;

public class LoginCommand : IRequest<AuthResponseDto>
{
    public required LoginDto LoginData { get; init; }
}

