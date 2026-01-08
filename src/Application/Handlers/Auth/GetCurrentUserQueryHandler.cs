using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Application.Queries.Auth;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Auth;

public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly ITokenExtractor _tokenExtractor;
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrentUserQueryHandler(ITokenExtractor tokenExtractor, IUnitOfWork unitOfWork)
    {
        _tokenExtractor = tokenExtractor ?? throw new ArgumentNullException(nameof(tokenExtractor));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _tokenExtractor.ExtractUserId();
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("Token not found or invalid");

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is not active");

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email.Value,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}

