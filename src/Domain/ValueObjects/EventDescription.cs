namespace AgendaManager.Domain.ValueObjects;

public sealed class EventDescription : IEquatable<EventDescription>
{
    public string Value { get; }

    private EventDescription(string value)
    {
        Value = value;
    }

    public static EventDescription Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return new EventDescription(string.Empty);

        if (description.Trim().Length > 1000)
            throw new ArgumentException("Event description cannot be longer than 1000 characters", nameof(description));

        return new EventDescription(description.Trim());
    }

    public bool Equals(EventDescription? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EventDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(EventDescription? left, EventDescription? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(EventDescription? left, EventDescription? right)
    {
        return !(left == right);
    }
}
