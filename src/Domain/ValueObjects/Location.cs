namespace AgendaManager.Domain.ValueObjects;

public sealed class Location : IEquatable<Location>
{
    public string Value { get; }

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

    public bool Equals(Location? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Location other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(Location? left, Location? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Location? left, Location? right)
    {
        return !(left == right);
    }
}
