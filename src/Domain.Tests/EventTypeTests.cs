using AgendaManager.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AgendaManager.Domain.Tests;

public class EventTypeTests
{
    [Fact]
    public void Exclusive_HasValueZero()
    {
        ((int)EventType.Exclusive).Should().Be(0);
    }

    [Fact]
    public void Shared_HasValueOne()
    {
        ((int)EventType.Shared).Should().Be(1);
    }

    [Fact]
    public void Exclusive_IsDifferentFromShared()
    {
        EventType.Exclusive.Should().NotBe(EventType.Shared);
    }
}
