using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Application.Queries.Events;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class GetEventByIdQueryHandler : BaseHandler, IQueryHandler<GetEventByIdQuery, EventDto>
{
    public GetEventByIdQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventId);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (!await _unitOfWork.Events.CanUserViewEventAsync(request.EventId, request.UserId))
            throw new UnauthorizedAccessException("User cannot view this event");

        return MapToDto(eventEntity);
    }

    private EventDto MapToDto(Domain.Entities.Event eventEntity)
    {
        return new EventDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name.Value,
            Description = eventEntity.Description.Value,
            Date = eventEntity.Date,
            Location = eventEntity.Location.Value,
            Type = eventEntity.Type,
            IsActive = eventEntity.IsActive,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            CreatorId = eventEntity.CreatorId,
            CreatorName = eventEntity.Creator?.Name ?? string.Empty,
            Participants = eventEntity.Participants?.Select(p => new ParticipantDto
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email.Value
            }).ToList() ?? new List<ParticipantDto>()
        };
    }
}
