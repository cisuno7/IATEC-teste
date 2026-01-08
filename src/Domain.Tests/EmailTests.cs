using AgendaManager.Domain.ValueObjects;

namespace AgendaManager.Domain.Tests;

public class EmailTests
{
    [Fact]
    public void Create_ValidEmail_ReturnsEmail()
    {
        var emailString = "test@example.com";
        var email = Email.Create(emailString);

        Assert.Equal(emailString.ToLower(), email.Value);
    }

    [Fact]
    public void Create_EmptyEmail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Email.Create(""));
        Assert.Throws<ArgumentException>(() => Email.Create("   "));
    }

    [Fact]
    public void Create_InvalidEmailFormat_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Email.Create("invalid-email"));
        Assert.Throws<ArgumentException>(() => Email.Create("email@"));
    }

    [Fact]
    public void Create_EmailTooLong_ThrowsArgumentException()
    {
        var longEmail = new string('a', 140) + "@example.com";
        Assert.Throws<ArgumentException>(() => Email.Create(longEmail));
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        Assert.True(email1.Equals(email2));
        Assert.True(email1 == email2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        Assert.False(email1.Equals(email2));
        Assert.True(email1 != email2);
    }
}
