using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Enums;

namespace AgendaManager.Domain.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetEventsByUserAsync(Guid userId, bool includeInactive = false);
    Task<IEnumerable<Event>> GetEventsByCreatorAsync(Guid creatorId, bool includeInactive = false);
    Task<IEnumerable<Event>> GetEventsByParticipantAsync(Guid participantId, bool includeInactive = false);
    Task<IEnumerable<Event>> GetFilteredEventsAsync(Guid userId, DateTime? startDate = null,
                                                   DateTime? endDate = null, string? searchText = null,
                                                   bool includeInactive = false);
    Task<IEnumerable<Event>> GetTodayEventsAsync(Guid userId, bool includeInactive = true);
    Task<IEnumerable<Event>> GetWeekEventsAsync(Guid userId, bool includeInactive = true);
    Task<IEnumerable<Event>> GetMonthEventsAsync(Guid userId, bool includeInactive = true);
    Task<IEnumerable<Event>> GetEventsByTypeAsync(Guid userId, EventType type, bool includeInactive = false);
    Task<bool> CanUserViewEventAsync(Guid eventId, Guid userId);
    Task<bool> CanUserEditEventAsync(Guid eventId, Guid userId);
}
