using AgendaManager.Application.Commands.Events;
using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class UpdateEventCommandHandler : BaseHandler, ICommandHandler<UpdateEventCommand, EventDto>
{
    public UpdateEventCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        : base(unitOfWork, dateTimeProvider)
    {
    }

    public async Task<EventDto> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(request.EventData.Id);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (!await _unitOfWork.Events.CanUserEditEventAsync(request.EventData.Id, request.UserId))
            throw new UnauthorizedAccessException("User cannot edit this event");

        var participants = new List<User>();
        if (request.EventData.Type == Domain.Enums.EventType.Shared && request.EventData.ParticipantIds.Any())
        {
            participants = (await _unitOfWork.Users.GetByIdsAsync(request.EventData.ParticipantIds)).ToList();

            var invalidParticipants = request.EventData.ParticipantIds.Except(participants.Select(p => p.Id));
            if (invalidParticipants.Any())
                throw new InvalidOperationException($"Invalid participant IDs: {string.Join(", ", invalidParticipants)}");

            if (participants.Any(p => !p.IsActive))
                throw new InvalidOperationException("Some participants are not active");
        }

        eventEntity.Update(
            request.EventData.Name,
            request.EventData.Description,
            _dateTimeProvider.ToUtc(request.EventData.Date),
            request.EventData.Location,
            request.EventData.Type
        );

        eventEntity.UpdateParticipants(participants);

        await _unitOfWork.Events.UpdateAsync(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(eventEntity);
    }

    private EventDto MapToDto(Event eventEntity)
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
