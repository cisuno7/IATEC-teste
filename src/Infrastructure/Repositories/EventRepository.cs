using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Enums;
using AgendaManager.Domain.Interfaces;
using AgendaManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgendaManager.Infrastructure.Repositories;




























































































































































public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetEventsByUserAsync(Guid userId, bool includeInactive = false)
    {
        var query = _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Participants)
            .Where(e => (e.CreatorId == userId || e.Participants.Any(p => p.Id == userId)));

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsByCreatorAsync(Guid creatorId, bool includeInactive = false)
    {
        var query = _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Participants)
            .Where(e => e.CreatorId == creatorId);

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsByParticipantAsync(Guid participantId, bool includeInactive = false)
    {
        var query = _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Participants)
            .Where(e => e.Participants.Any(p => p.Id == participantId));

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetFilteredEventsAsync(Guid userId, DateTime? startDate = null,
                                                                DateTime? endDate = null, string? searchText = null,
                                                                bool includeInactive = false)
    {
        var query = _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Participants)
            .Where(e => e.CreatorId == userId || e.Participants.Any(p => p.Id == userId));

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        if (startDate.HasValue)
        {
            query = query.Where(e => e.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.Date <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var searchTerm = searchText.Trim().ToLower();
            query = query.Where(e =>
                e.Name.Value.ToLower().Contains(searchTerm) ||
                e.Description.Value.ToLower().Contains(searchTerm) ||
                e.Location.Value.ToLower().Contains(searchTerm));
        }

        return await query
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetTodayEventsAsync(Guid userId, bool includeInactive = true)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await GetFilteredEventsAsync(userId, today, tomorrow.AddSeconds(-1), null, includeInactive);
    }

    public async Task<IEnumerable<Event>> GetWeekEventsAsync(Guid userId, bool includeInactive = true)
    {
        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

        return await GetFilteredEventsAsync(userId, startOfWeek, endOfWeek, null, includeInactive);
    }

    public async Task<IEnumerable<Event>> GetMonthEventsAsync(Guid userId, bool includeInactive = true)
    {
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

        return await GetFilteredEventsAsync(userId, startOfMonth, endOfMonth, null, includeInactive);
    }

    public async Task<IEnumerable<Event>> GetEventsByTypeAsync(Guid userId, EventType type, bool includeInactive = false)
    {
        var query = _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Participants)
            .Where(e => (e.CreatorId == userId || e.Participants.Any(p => p.Id == userId))
                       && e.Type == type);

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<bool> CanUserViewEventAsync(Guid eventId, Guid userId)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
            return false;

        return eventEntity.CanUserView(userId);
    }

    public async Task<bool> CanUserEditEventAsync(Guid eventId, Guid userId)
    {
        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
            return false;

        return eventEntity.CanUserEdit(userId);
    }
}
