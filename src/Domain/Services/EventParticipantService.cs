using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Interfaces;

namespace AgendaManager.Domain.Services;

public class EventParticipantService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public EventParticipantService(IEventRepository eventRepository, IUserRepository userRepository)
    {
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task AddParticipantAsync(Guid eventId, Guid userId, Guid creatorId)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(eventId);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (eventEntity.CreatorId != creatorId)
            throw new UnauthorizedAccessException("Only event creator can manage participants");

        if (eventEntity.Type == Enums.EventType.Exclusive)
            throw new InvalidOperationException("Cannot add participants to exclusive events");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        if (!user.IsActive)
            throw new InvalidOperationException("Cannot add inactive user as participant");

        eventEntity.AddParticipant(user);
    }

    public async Task RemoveParticipantAsync(Guid eventId, Guid userId, Guid creatorId)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(eventId);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (eventEntity.CreatorId != creatorId)
            throw new UnauthorizedAccessException("Only event creator can manage participants");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        eventEntity.RemoveParticipant(user);
    }

    public async Task<IEnumerable<User>> GetAvailableParticipantsAsync(Guid eventId, Guid creatorId)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(eventId);
        if (eventEntity is null)
            throw new InvalidOperationException("Event not found");

        if (eventEntity.CreatorId != creatorId)
            throw new UnauthorizedAccessException("Unauthorized access");

        var allActiveUsers = await _userRepository.GetActiveUsersAsync();
        var existingParticipantIds = eventEntity.Participants.Select(p => p.Id).ToHashSet();

        return allActiveUsers.Where(u => u.Id != creatorId && !existingParticipantIds.Contains(u.Id));
    }
}
