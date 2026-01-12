using AgendaManager.Application.DTOs;
using AgendaManager.Application.Exceptions;
using AgendaManager.Application.Interfaces;
using AgendaManager.Application.Queries.Events;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Application.Handlers.Events;

public class GetDashboardEventsQueryHandler : BaseHandler, IQueryHandler<GetDashboardEventsQuery, IEnumerable<EventDto>>
{
    public GetDashboardEventsQueryHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        : base(unitOfWork, dateTimeProvider)
    {
    }

    public async Task<IEnumerable<EventDto>> Handle(GetDashboardEventsQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        if (!user.IsActive)
            throw new InvalidOperationException("User is not active");

        var (startDateTime, endDateTime) = ResolveDateRange(request);

        ValidateDateRange(startDateTime, endDateTime);

        var events = await _unitOfWork.Events.GetFilteredEventsAsync(
            request.UserId,
            startDateTime,
            endDateTime,
            request.SearchText,
            includeInactive: true);

        return events.Select(MapToDto);
    }

    private (DateTime?, DateTime?) ResolveDateRange(GetDashboardEventsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.PeriodType))
        {
            return ResolvePeriodType(request.PeriodType);
        }

        var normalizedStartDate = _dateTimeProvider.ToUtc(request.StartDate);
        var normalizedEndDate = _dateTimeProvider.ToUtc(request.EndDate);

        var startDateTime = CombineDateAndTime(normalizedStartDate, request.StartTime);
        var endDateTime = CombineDateAndTime(normalizedEndDate, request.EndTime);

        return (startDateTime, endDateTime);
    }

    private (DateTime?, DateTime?) ResolvePeriodType(string periodType)
    {
        var now = _dateTimeProvider.UtcNow;
        var today = _dateTimeProvider.ToUtcDateOnly(now);

        return periodType.ToLower() switch
        {
            "today" => (today, today.AddDays(1).AddSeconds(-1)),
            "week" => (GetStartOfWeek(today), GetStartOfWeek(today).AddDays(7).AddSeconds(-1)),
            "month" => (GetStartOfMonth(today), GetStartOfMonth(today).AddMonths(1).AddSeconds(-1)),
            _ => (null, null)
        };
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
        var startOfWeek = date.AddDays(-1 * diff);
        return new DateTime(startOfWeek.Year, startOfWeek.Month, startOfWeek.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private DateTime GetStartOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    private void ValidateDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue)
            return;

        if (endDate.Value < startDate.Value)
            throw new InvalidDateRangeException("End date cannot be earlier than start date");
    }

    private DateTime? CombineDateAndTime(DateTime? date, string? time)
    {
        if (!date.HasValue && string.IsNullOrWhiteSpace(time))
            return null;

        if (!date.HasValue)
            return null;

        var normalizedDate = _dateTimeProvider.ToUtc(date.Value);
        
        if (string.IsNullOrWhiteSpace(time))
            return normalizedDate;

        if (TimeSpan.TryParse(time, out var timeSpan))
        {
            var dateOnly = new DateTime(normalizedDate.Year, normalizedDate.Month, normalizedDate.Day, 0, 0, 0, DateTimeKind.Utc);
            return dateOnly.Add(timeSpan);
        }

        return normalizedDate;
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
