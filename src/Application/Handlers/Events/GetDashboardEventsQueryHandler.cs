using AgendaManager.Application.DTOs;
using AgendaManager.Application.Interfaces;
using AgendaManager.Application.Queries.Events;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class GetDashboardEventsQueryHandler : BaseHandler, IQueryHandler<GetDashboardEventsQuery, IEnumerable<EventDto>>
{
    public GetDashboardEventsQueryHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<EventDto>> Handle(GetDashboardEventsQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        if (!user.IsActive)
            throw new InvalidOperationException("User is not active");

        IEnumerable<Domain.Entities.Event> events;

        switch (request.PeriodType?.ToLower())
        {
            case "today":
                events = await _unitOfWork.Events.GetTodayEventsAsync(request.UserId);
                break;

            case "week":
                events = await _unitOfWork.Events.GetWeekEventsAsync(request.UserId);
                break;

            case "month":
                events = await _unitOfWork.Events.GetMonthEventsAsync(request.UserId);
                break;

            default:
                events = await _unitOfWork.Events.GetFilteredEventsAsync(
                    request.UserId,
                    request.StartDate,
                    request.EndDate,
                    request.SearchText);
                break;
        }

        return events.Select(MapToDto);
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
