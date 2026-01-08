using MediatR;

namespace AgendaManager.Application.Queries.Users;

public class GetActiveUsersQuery : IRequest<IEnumerable<Application.DTOs.UserDto>>
{
    public Guid ExcludeUserId { get; }

    public GetActiveUsersQuery(Guid excludeUserId)
    {
        ExcludeUserId = excludeUserId;
    }
}
