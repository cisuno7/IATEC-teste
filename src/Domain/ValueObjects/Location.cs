namespace AgendaManager.Domain.ValueObjects;

public class Location
{
    public string Value { get; private set; }

    private Location(string value)
    {
        Value = value;
    }

    public static Location Create(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return new Location(string.Empty);

        if (location.Trim().Length > 300)
            throw new ArgumentException("Location cannot be longer than 300 characters", nameof(location));

        return new Location(location.Trim());
    }

    public override bool Equals(object? obj)
    {
        if (obj is Location location)
            return Value == location.Value;
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
