using AgendaManager.Application.Commands.Auth;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Interfaces;
using MediatR;

namespace AgendaManager.Application.Handlers.Auth;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResult>
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IAuthService authService, IUnitOfWork unitOfWork)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(AgendaManager.Domain.ValueObjects.Email.Create(request.RegisterData.Email));
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        var passwordHash = _authService.HashPassword(request.RegisterData.Password);
        var user = User.Create(request.RegisterData.Name, request.RegisterData.Email, passwordHash);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new RegisterResult
        {
            UserId = user.Id.ToString(),
            Email = user.Email.Value
        };
    }
}

