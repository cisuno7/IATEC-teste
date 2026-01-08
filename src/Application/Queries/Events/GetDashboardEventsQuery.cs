using AgendaManager.Application.DTOs;
using MediatR;

namespace AgendaManager.Application.Queries.Events;

public class GetDashboardEventsQuery : IRequest<IEnumerable<EventDto>>
{
    public Guid UserId { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }
    public string? SearchText { get; }
    public string? PeriodType { get; }

    public GetDashboardEventsQuery(Guid userId, DateTime? startDate = null, DateTime? endDate = null,
                                   string? searchText = null, string? periodType = null)
    {
        UserId = userId != Guid.Empty ? userId : throw new ArgumentException("User ID cannot be empty", nameof(userId));
        StartDate = startDate;
        EndDate = endDate;
        SearchText = searchText;
        PeriodType = periodType;
    }
}
