namespace AgendaManager.Domain.ValueObjects;

public class EventName
{
    public string Value { get; private set; }

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

    public override bool Equals(object? obj)
    {
        if (obj is EventName eventName)
            return Value == eventName.Value;
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
