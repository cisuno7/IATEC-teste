using AgendaManager.Domain.ValueObjects;

namespace AgendaManager.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<Event> CreatedEvents { get; private set; } = null!;
    public ICollection<Event> ParticipatedEvents { get; private set; } = null!;

    private User() { }

    public static User Create(string name, string email, string passwordHash)
    {
        ValidateName(name);
        ValidatePassword(passwordHash);

        return new User
        {
            Name = name.Trim(),
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedEvents = new List<Event>(),
            ParticipatedEvents = new List<Event>()
        };
    }

    public void Update(string name, string email)
    {
        ValidateName(name);
        Name = name.Trim();
        Email = Email.Create(email);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Trim().Length < 2)
            throw new ArgumentException("Name must be at least 2 characters long", nameof(name));

        if (name.Trim().Length > 100)
            throw new ArgumentException("Name cannot be longer than 100 characters", nameof(name));
    }

    private static void ValidatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));
    }
}
