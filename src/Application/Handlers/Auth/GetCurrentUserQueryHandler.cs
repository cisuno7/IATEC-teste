using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Application.Queries.Auth;

namespace AgendaManager.Application.Handlers.Auth;

public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IAuthService _authService;
    private readonly ITokenExtractor _tokenExtractor;

    public GetCurrentUserQueryHandler(IAuthService authService, ITokenExtractor tokenExtractor)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _tokenExtractor = tokenExtractor ?? throw new ArgumentNullException(nameof(tokenExtractor));
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var token = _tokenExtractor.ExtractToken();
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Token not found");

        var authUser = await _authService.GetUserAsync(token);
        if (authUser == null)
            throw new UnauthorizedAccessException("Invalid user");

        if (!Guid.TryParse(authUser.Id, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID");

        return new UserDto
        {
            Id = userId,
            Name = authUser.Name,
            Email = authUser.Email,
            IsActive = true,
            CreatedAt = authUser.CreatedAt
        };
    }
}

