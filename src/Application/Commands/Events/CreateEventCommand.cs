using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Commands.Events;

public class CreateEventCommand : IRequest<EventDto>
{
    public CreateEventDto EventData { get; }
    public Guid CreatorId { get; }

    public CreateEventCommand(CreateEventDto eventData, Guid creatorId)
    {
        EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));
        CreatorId = creatorId != Guid.Empty ? creatorId : throw new ArgumentException("Creator ID cannot be empty", nameof(creatorId));
    }
}
