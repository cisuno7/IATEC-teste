namespace AgendaManager.Domain.ValueObjects;

public class Email
{
    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!email.Contains("@") || !email.Contains("."))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (email.Length > 150)
            throw new ArgumentException("Email cannot be longer than 150 characters", nameof(email));

        return new Email(email.ToLower().Trim());
    }

    public override bool Equals(object? obj)
    {
        if (obj is Email email)
            return Value == email.Value;
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
