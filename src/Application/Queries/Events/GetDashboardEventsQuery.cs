using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Queries.Events;

public class GetDashboardEventsQuery : IRequest<IEnumerable<EventDto>>
{
    public Guid UserId { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }
    public string? StartTime { get; }
    public string? EndTime { get; }
    public string? SearchText { get; }
    public string? PeriodType { get; }

    public GetDashboardEventsQuery(Guid userId, DateTime? startDate = null, DateTime? endDate = null,
                                   string? startTime = null, string? endTime = null,
                                   string? searchText = null, string? periodType = null)
    {
        UserId = userId != Guid.Empty ? userId : throw new ArgumentException("User ID cannot be empty", nameof(userId));
        StartDate = startDate;
        EndDate = endDate;
        StartTime = startTime;
        EndTime = endTime;
        SearchText = searchText;
        PeriodType = periodType;
    }
}
