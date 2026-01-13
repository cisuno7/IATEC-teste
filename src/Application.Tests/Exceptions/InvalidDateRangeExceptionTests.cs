using AgendaManager.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AgendaManager.Application.Tests.Exceptions;

public class InvalidDateRangeExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var message = "End date cannot be earlier than start date";

        var exception = new InvalidDateRangeException(message);

        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessage_InheritsFromException()
    {
        var message = "Test message";

        var exception = new InvalidDateRangeException(message);

        exception.Should().BeAssignableTo<Exception>();
    }
}
