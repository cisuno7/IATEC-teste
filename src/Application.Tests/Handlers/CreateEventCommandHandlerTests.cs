using AgendaManager.Application.Commands.Events;
using AgendaManager.Application.DTOs;
using AgendaManager.Application.Handlers.Events;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Enums;
using AgendaManager.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgendaManager.Application.Tests.Handlers;

public class CreateEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new CreateEventCommandHandler(_unitOfWorkMock.Object, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesExclusiveEvent_WithValidData()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var normalizedDate = DateTime.UtcNow.AddDays(1).ToUniversalTime();

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        var eventDto = new CreateEventDto
        {
            Name = "Test Event",
            Description = "Test Description",
            Date = futureDate,
            Location = "Test Location",
            Type = EventType.Exclusive,
            ParticipantIds = new List<Guid>()
        };

        var command = new CreateEventCommand(eventDto, creatorId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(creatorId))
            .ReturnsAsync(creator);

        _dateTimeProviderMock.Setup(p => p.ToUtc(futureDate))
            .Returns(normalizedDate);

        Event? savedEvent = null;
        _eventRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Event>()))
            .Callback<Event>(e => savedEvent = e)
            .Returns(Task.CompletedTask);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => savedEvent);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Event");
        result.Type.Should().Be(EventType.Exclusive);
        result.CreatorId.Should().Be(creatorId);
        result.Participants.Should().BeEmpty();

        _eventRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CreatesSharedEvent_WithParticipants()
    {
        var creatorId = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var participantId2 = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var normalizedDate = DateTime.UtcNow.AddDays(1).ToUniversalTime();

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        var participant1 = User.CreateWithId(participantId1, "Participant1", "p1@test.com", "hash");
        var participant2 = User.CreateWithId(participantId2, "Participant2", "p2@test.com", "hash");

        var eventDto = new CreateEventDto
        {
            Name = "Shared Event",
            Description = "Shared Description",
            Date = futureDate,
            Location = "Location",
            Type = EventType.Shared,
            ParticipantIds = new List<Guid> { participantId1, participantId2 }
        };

        var command = new CreateEventCommand(eventDto, creatorId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(creatorId))
            .ReturnsAsync(creator);

        _userRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<User> { participant1, participant2 });

        _dateTimeProviderMock.Setup(p => p.ToUtc(futureDate))
            .Returns(normalizedDate);

        Event? savedEvent = null;
        _eventRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Event>()))
            .Callback<Event>(e => savedEvent = e)
            .Returns(Task.CompletedTask);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => savedEvent);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Type.Should().Be(EventType.Shared);
        result.Participants.Should().HaveCount(2);

        _userRepositoryMock.Verify(r => r.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.Count == 2)), Times.Once);
    }

    [Fact]
    public async Task Handle_NormalizesDateTime_ToUtc()
    {
        var creatorId = Guid.NewGuid();
        var localDate = DateTime.Now;
        var utcDate = localDate.ToUniversalTime();

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        var eventDto = new CreateEventDto
        {
            Name = "Test Event",
            Description = "Description",
            Date = localDate,
            Location = "Location",
            Type = EventType.Exclusive,
            ParticipantIds = new List<Guid>()
        };

        var command = new CreateEventCommand(eventDto, creatorId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(creatorId))
            .ReturnsAsync(creator);

        _dateTimeProviderMock.Setup(p => p.ToUtc(localDate))
            .Returns(utcDate);

        Event? savedEvent = null;
        _eventRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Event>()))
            .Callback<Event>(e => savedEvent = e)
            .Returns(Task.CompletedTask);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => savedEvent);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _dateTimeProviderMock.Verify(p => p.ToUtc(localDate), Times.Once);
    }

    [Fact]
    public async Task Handle_CreatorNotFound_ThrowsInvalidOperationException()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventDto = new CreateEventDto
        {
            Name = "Test Event",
            Description = "Description",
            Date = futureDate,
            Location = "Location",
            Type = EventType.Exclusive,
            ParticipantIds = new List<Guid>()
        };

        var command = new CreateEventCommand(eventDto, creatorId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(creatorId))
            .ReturnsAsync((User?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Creator user not found");
    }

    [Fact]
    public async Task Handle_CreatorInactive_ThrowsInvalidOperationException()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        creator.Deactivate();

        var eventDto = new CreateEventDto
        {
            Name = "Test Event",
            Description = "Description",
            Date = futureDate,
            Location = "Location",
            Type = EventType.Exclusive,
            ParticipantIds = new List<Guid>()
        };

        var command = new CreateEventCommand(eventDto, creatorId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(creatorId))
            .ReturnsAsync(creator);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Creator user is not active");
    }
}
