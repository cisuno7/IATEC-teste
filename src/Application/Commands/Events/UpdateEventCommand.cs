using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Commands.Events;

public class UpdateEventCommand : IRequest<EventDto>
{
    public UpdateEventDto EventData { get; }
    public Guid UserId { get; }

    public UpdateEventCommand(UpdateEventDto eventData, Guid userId)
    {
        EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));
        UserId = userId != Guid.Empty ? userId : throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }
}
