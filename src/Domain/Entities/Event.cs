using AgendaManager.Domain.Enums;
using AgendaManager.Domain.ValueObjects;

namespace AgendaManager.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public EventName Name { get; private set; } = null!;
    public EventDescription Description { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public Location Location { get; private set; } = null!;
    public EventType Type { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatorId { get; private set; }
    public User Creator { get; private set; } = null!;
    public ICollection<User> Participants { get; private set; } = null!;

    private Event() { }

    public static Event Create(string name, string description, DateTime date, string location, EventType type, Guid creatorId)
    {
        ValidateDate(date);
        ValidateCreatorId(creatorId);

        return new Event
        {
            Name = EventName.Create(name),
            Description = EventDescription.Create(description),
            Date = date,
            Location = Location.Create(location),
            Type = type,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatorId = creatorId,
            Participants = new List<User>()
        };
    }

    public void Update(string name, string description, DateTime date, string location, EventType type)
    {
        ValidateDate(date);

        Name = EventName.Create(name);
        Description = EventDescription.Create(description);
        Date = date;
        Location = Location.Create(location);
        Type = type;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddParticipant(User participant)
    {
        if (participant is null)
            throw new ArgumentNullException(nameof(participant));

        if (!Participants.Contains(participant))
            Participants.Add(participant);
    }

    public void RemoveParticipant(User participant)
    {
        if (participant is null)
            throw new ArgumentNullException(nameof(participant));

        Participants.Remove(participant);
    }

    public void UpdateParticipants(IEnumerable<User> participants)
    {
        if (participants is null)
            throw new ArgumentNullException(nameof(participants));

        Participants.Clear();
        foreach (var participant in participants)
        {
            AddParticipant(participant);
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanUserView(Guid userId)
    {
        if (CreatorId == userId)
            return true;

        if (Type == EventType.Shared)
            return Participants.Any(p => p.Id == userId);

        return false;
    }

    public bool CanUserEdit(Guid userId)
    {
        return CreatorId == userId;
    }

    private static void ValidateDate(DateTime date)
    {
        if (date < DateTime.UtcNow.AddMinutes(-1))
            throw new ArgumentException("Event date cannot be in the past", nameof(date));
    }

    private static void ValidateCreatorId(Guid creatorId)
    {
        if (creatorId == Guid.Empty)
            throw new ArgumentException("Creator ID cannot be empty", nameof(creatorId));
    }
}
