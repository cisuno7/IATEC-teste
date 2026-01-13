using AgendaManager.Application.Commands.Auth;
using AgendaManager.Application.Handlers.Auth;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Interfaces;
using AgendaManager.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgendaManager.Application.Tests.Handlers.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new RegisterCommandHandler(_authServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidData_CreatesUser()
    {
        var email = "newuser@test.com";
        var password = "password123";
        var name = "New User";
        var passwordHash = "hashed_password";

        var registerDto = new Application.DTOs.RegisterDto
        {
            Email = email,
            Password = password,
            Name = name
        };

        var command = new RegisterCommand { RegisterData = registerDto };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        _authServiceMock.Setup(s => s.HashPassword(password))
            .Returns(passwordHash);

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.User>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.UserId.Should().NotBeNullOrEmpty();

        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Entities.User>(u =>
            u.Email.Value == email &&
            u.Name == name &&
            u.PasswordHash == passwordHash)), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var email = "existing@test.com";
        var password = "password123";
        var name = "Existing User";
        var passwordHash = "hashed_password";

        var existingUser = Domain.Entities.User.CreateWithId(userId, name, email, passwordHash);

        var registerDto = new Application.DTOs.RegisterDto
        {
            Email = email,
            Password = password,
            Name = "New User"
        };

        var command = new RegisterCommand { RegisterData = registerDto };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(existingUser);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
