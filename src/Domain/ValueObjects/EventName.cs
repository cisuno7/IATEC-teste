namespace AgendaManager.Domain.ValueObjects;

public sealed class EventName : IEquatable<EventName>
{
    public string Value { get; }

    private EventName(string value)
    {
        Value = value;
    }

    public static EventName Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be empty", nameof(name));

        if (name.Trim().Length > 200)
            throw new ArgumentException("Event name cannot be longer than 200 characters", nameof(name));

        return new EventName(name.Trim());
    }

    public bool Equals(EventName? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EventName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(EventName? left, EventName? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(EventName? left, EventName? right)
    {
        return !(left == right);
    }
}
