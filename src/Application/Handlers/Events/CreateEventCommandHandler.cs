using AgendaManager.Application.Commands.Events;
using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class CreateEventCommandHandler : BaseHandler, ICommandHandler<CreateEventCommand, EventDto>
{
    public CreateEventCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        : base(unitOfWork, dateTimeProvider)
    {
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var creator = await _unitOfWork.Users.GetByIdAsync(request.CreatorId);
        if (creator is null)
            throw new InvalidOperationException("Creator user not found");

        if (!creator.IsActive)
            throw new InvalidOperationException("Creator user is not active");

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

        var eventEntity = Event.Create(
            request.EventData.Name,
            request.EventData.Description,
            _dateTimeProvider.ToUtc(request.EventData.Date),
            request.EventData.Location,
            request.EventData.Type,
            request.CreatorId
        );

        foreach (var participant in participants)
        {
            eventEntity.AddParticipant(participant);
        }

        await _unitOfWork.Events.AddAsync(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        var savedEvent = await _unitOfWork.Events.GetByIdAsync(eventEntity.Id);
        if (savedEvent is null)
            throw new InvalidOperationException("Failed to retrieve created event");

        return MapToDto(savedEvent);
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
