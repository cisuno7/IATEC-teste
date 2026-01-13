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

public class UpdateEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly UpdateEventCommandHandler _handler;

    public UpdateEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new UpdateEventCommandHandler(_unitOfWorkMock.Object, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_CreatorEditsEvent_UpdatesSuccessfully()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var newDate = DateTime.UtcNow.AddDays(2);
        var normalizedDate = newDate.ToUniversalTime();

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        var eventEntity = Event.Create(
            "Original Name",
            "Original Description",
            futureDate,
            "Original Location",
            EventType.Exclusive,
            creatorId
        );

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Name",
            Description = "Updated Description",
            Date = newDate,
            Location = "Updated Location",
            Type = EventType.Shared,
            ParticipantIds = new List<Guid>()
        };

        var command = new UpdateEventCommand(updateDto, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _dateTimeProviderMock.Setup(p => p.ToUtc(newDate))
            .Returns(normalizedDate);

        _eventRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        _eventRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ParticipantCannotEdit_ThrowsUnauthorizedAccessException()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Name",
            Description = "Updated Description",
            Date = futureDate,
            Location = "Location",
            Type = EventType.Shared,
            ParticipantIds = new List<Guid>()
        };

        var command = new UpdateEventCommand(updateDto, participantId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, participantId))
            .ReturnsAsync(false);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User cannot edit this event");
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsInvalidOperationException()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Name",
            Description = "Description",
            Date = futureDate,
            Location = "Location",
            Type = EventType.Exclusive,
            ParticipantIds = new List<Guid>()
        };

        var command = new UpdateEventCommand(updateDto, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Event not found");
    }

    [Fact]
    public async Task Handle_UpdatesParticipants_WhenEditingSharedEvent()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var participantId2 = Guid.NewGuid();
        var participantId3 = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var normalizedDate = futureDate.ToUniversalTime();

        var participant1 = User.CreateWithId(participantId1, "Participant1", "p1@test.com", "hash");
        var participant2 = User.CreateWithId(participantId2, "Participant2", "p2@test.com", "hash");
        var participant3 = User.CreateWithId(participantId3, "Participant3", "p3@test.com", "hash");

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        eventEntity.AddParticipant(participant1);
        eventEntity.AddParticipant(participant2);

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Event",
            Description = "Updated Description",
            Date = futureDate,
            Location = "Updated Location",
            Type = EventType.Shared,
            ParticipantIds = new List<Guid> { participantId2, participantId3 }
        };

        var command = new UpdateEventCommand(updateDto, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _userRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<User> { participant2, participant3 });

        _dateTimeProviderMock.Setup(p => p.ToUtc(futureDate))
            .Returns(normalizedDate);

        _eventRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Participants.Should().HaveCount(2);
        result.Participants.Should().Contain(p => p.Id == participantId2);
        result.Participants.Should().Contain(p => p.Id == participantId3);
        _eventRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RemovesAllParticipants_WhenChangingToExclusive()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var participantId2 = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var normalizedDate = futureDate.ToUniversalTime();

        var participant1 = User.CreateWithId(participantId1, "Participant1", "p1@test.com", "hash");
        var participant2 = User.CreateWithId(participantId2, "Participant2", "p2@test.com", "hash");

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        eventEntity.AddParticipant(participant1);
        eventEntity.AddParticipant(participant2);

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Event",
            Description = "Updated Description",
            Date = futureDate,
            Location = "Updated Location",
            Type = EventType.Exclusive,
            ParticipantIds = new List<Guid>()
        };

        var command = new UpdateEventCommand(updateDto, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _dateTimeProviderMock.Setup(p => p.ToUtc(futureDate))
            .Returns(normalizedDate);

        _eventRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Type.Should().Be(EventType.Exclusive);
        result.Participants.Should().BeEmpty();
        _eventRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AddsParticipants_WhenChangingToShared()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var participantId2 = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var normalizedDate = futureDate.ToUniversalTime();

        var participant1 = User.CreateWithId(participantId1, "Participant1", "p1@test.com", "hash");
        var participant2 = User.CreateWithId(participantId2, "Participant2", "p2@test.com", "hash");

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Event",
            Description = "Updated Description",
            Date = futureDate,
            Location = "Updated Location",
            Type = EventType.Shared,
            ParticipantIds = new List<Guid> { participantId1, participantId2 }
        };

        var command = new UpdateEventCommand(updateDto, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _userRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<User> { participant1, participant2 });

        _dateTimeProviderMock.Setup(p => p.ToUtc(futureDate))
            .Returns(normalizedDate);

        _eventRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Type.Should().Be(EventType.Shared);
        result.Participants.Should().HaveCount(2);
        _eventRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidParticipantIds_ThrowsInvalidOperationException()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var invalidParticipantId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var participant1 = User.CreateWithId(participantId1, "Participant1", "p1@test.com", "hash");

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Event",
            Description = "Updated Description",
            Date = futureDate,
            Location = "Updated Location",
            Type = EventType.Shared,
            ParticipantIds = new List<Guid> { participantId1, invalidParticipantId }
        };

        var command = new UpdateEventCommand(updateDto, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _userRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<User> { participant1 });

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Invalid participant IDs: {invalidParticipantId}");
    }

    [Fact]
    public async Task Handle_InactiveParticipants_ThrowsInvalidOperationException()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var participant1 = User.CreateWithId(participantId1, "Participant1", "p1@test.com", "hash");
        participant1.Deactivate();

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Event",
            Description = "Updated Description",
            Date = futureDate,
            Location = "Updated Location",
            Type = EventType.Shared,
            ParticipantIds = new List<Guid> { participantId1 }
        };

        var command = new UpdateEventCommand(updateDto, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _userRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<User> { participant1 });

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Some participants are not active");
    }
}
