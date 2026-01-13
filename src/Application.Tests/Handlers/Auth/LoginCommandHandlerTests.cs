using AgendaManager.Application.Commands.Auth;
using AgendaManager.Application.DTOs;
using AgendaManager.Application.Handlers.Auth;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Interfaces;
using AgendaManager.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgendaManager.Application.Tests.Handlers.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new LoginCommandHandler(_authServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        var userId = Guid.NewGuid();
        var email = "user@test.com";
        var password = "password123";
        var passwordHash = "hashed_password";
        var token = "jwt_token";
        var name = "User";

        var user = User.CreateWithId(userId, name, email, passwordHash);

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        var command = new LoginCommand { LoginData = loginDto };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        _authServiceMock.Setup(s => s.VerifyPassword(password, passwordHash))
            .Returns(true);

        _authServiceMock.Setup(s => s.GenerateJwtToken(userId.ToString(), email, name))
            .Returns(token);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(userId);
        result.User.Email.Should().Be(email);
        result.User.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsUnauthorizedAccessException()
    {
        var email = "invalid@test.com";
        var password = "password123";

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        var command = new LoginCommand { LoginData = loginDto };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((User?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var email = "user@test.com";
        var password = "wrong_password";
        var passwordHash = "hashed_password";
        var name = "User";

        var user = User.CreateWithId(userId, name, email, passwordHash);

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        var command = new LoginCommand { LoginData = loginDto };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        _authServiceMock.Setup(s => s.VerifyPassword(password, passwordHash))
            .Returns(false);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var email = "user@test.com";
        var password = "password123";
        var passwordHash = "hashed_password";
        var name = "User";

        var user = User.CreateWithId(userId, name, email, passwordHash);
        user.Deactivate();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        var command = new LoginCommand { LoginData = loginDto };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not active");
    }
}
