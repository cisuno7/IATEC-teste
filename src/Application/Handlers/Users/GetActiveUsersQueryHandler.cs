using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Application.Queries.Users;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Users;

public class GetActiveUsersQueryHandler : BaseHandler, IQueryHandler<GetActiveUsersQuery, IEnumerable<UserDto>>
{
    public GetActiveUsersQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<UserDto>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetActiveUsersAsync();
        return users
            .Where(u => u.Id != request.ExcludeUserId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email.Value,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
    }
}
