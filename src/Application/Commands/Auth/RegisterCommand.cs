using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Commands.Auth;

public class RegisterCommand : IRequest<RegisterResult>
{
    public required RegisterDto RegisterData { get; init; }
}

public class RegisterResult
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
}

