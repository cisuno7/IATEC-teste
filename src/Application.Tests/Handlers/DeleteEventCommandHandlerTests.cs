using AgendaManager.Application.Commands.Events;
using AgendaManager.Application.Handlers.Events;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Enums;
using AgendaManager.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgendaManager.Application.Tests.Handlers;

public class DeleteEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly DeleteEventCommandHandler _handler;

    public DeleteEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);

        _handler = new DeleteEventCommandHandler(_unitOfWorkMock.Object, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_CreatorDeletesEvent_RemovesSuccessfully()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        var command = new DeleteEventCommand(eventId, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _eventRepositoryMock.Setup(r => r.RemoveAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _eventRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Event>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ParticipantCannotDelete_ThrowsUnauthorizedAccessException()
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

        var command = new DeleteEventCommand(eventId, participantId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, participantId))
            .ReturnsAsync(false);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User cannot delete this event");
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsInvalidOperationException()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var command = new DeleteEventCommand(eventId, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Event not found");
    }

    [Fact]
    public async Task Handle_RemovesAllRelations_WhenDeletingSharedEvent()
    {
        var creatorId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var participantId1 = Guid.NewGuid();
        var participantId2 = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var participant1 = User.CreateWithId(participantId1, "Participant1", "p1@test.com", "hash");
        var participant2 = User.CreateWithId(participantId2, "Participant2", "p2@test.com", "hash");

        var eventEntity = Event.Create(
            "Shared Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        eventEntity.AddParticipant(participant1);
        eventEntity.AddParticipant(participant2);

        var command = new DeleteEventCommand(eventId, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _eventRepositoryMock.Setup(r => r.RemoveAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask)
            .Callback<Event>(e =>
            {
                e.Participants.Should().HaveCount(2);
            });

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _eventRepositoryMock.Verify(r => r.RemoveAsync(It.Is<Event>(e => 
            e.Participants.Count == 2 && 
            e.Type == EventType.Shared)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
