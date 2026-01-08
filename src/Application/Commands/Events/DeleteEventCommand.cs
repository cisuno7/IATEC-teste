using MediatR;

namespace AgendaManager.Application.Commands.Events;

public class DeleteEventCommand : IRequest<bool>
{
    public Guid EventId { get; }
    public Guid UserId { get; }

    public DeleteEventCommand(Guid eventId, Guid userId)
    {
        EventId = eventId != Guid.Empty ? eventId : throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        UserId = userId != Guid.Empty ? userId : throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }
}
