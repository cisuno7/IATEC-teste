namespace AgendaManager.Domain.ValueObjects;

public class EventDescription
{
    public string Value { get; private set; }

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

    public override bool Equals(object? obj)
    {
        if (obj is EventDescription description)
            return Value == description.Value;
        return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}
