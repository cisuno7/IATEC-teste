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

public class ActivateEventCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ActivateEventCommandHandler _handler;

    public ActivateEventCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);

        _handler = new ActivateEventCommandHandler(_unitOfWorkMock.Object, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_CreatorActivatesEvent_ActivatesSuccessfully()
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

        eventEntity.Deactivate();

        var command = new ActivateEventCommand(eventId, creatorId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, creatorId))
            .ReturnsAsync(true);

        _eventRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        eventEntity.IsActive.Should().BeTrue();
        _eventRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ParticipantCannotActivate_ThrowsUnauthorizedAccessException()
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

        var command = new ActivateEventCommand(eventId, participantId);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.CanUserEditEventAsync(eventId, participantId))
            .ReturnsAsync(false);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User cannot edit this event");
    }
}
