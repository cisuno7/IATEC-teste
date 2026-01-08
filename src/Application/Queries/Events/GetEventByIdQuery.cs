using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Queries.Events;

public class GetEventByIdQuery : IRequest<EventDto>
{
    public Guid EventId { get; }
    public Guid UserId { get; }

    public GetEventByIdQuery(Guid eventId, Guid userId)
    {
        EventId = eventId != Guid.Empty ? eventId : throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
        UserId = userId != Guid.Empty ? userId : throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }
}
