using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Queries.Auth;

public class GetCurrentUserQuery : IRequest<UserDto>
{
}

