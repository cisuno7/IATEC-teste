using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Enums;
using AgendaManager.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AgendaManager.Domain.Tests;

public class EventTests
{
    [Fact]
    public void Create_ExclusiveEvent_CreatorIsOnlyOwner()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Test Event",
            "Test Description",
            futureDate,
            "Test Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.CreatorId.Should().Be(creatorId);
        eventEntity.Type.Should().Be(EventType.Exclusive);
        eventEntity.Participants.Should().BeEmpty();
        eventEntity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_SharedEvent_CanHaveParticipants()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        var participant = User.CreateWithId(participantId, "Participant", "participant@test.com", "hash");

        var eventEntity = Event.Create(
            "Shared Event",
            "Shared Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        eventEntity.AddParticipant(participant);

        eventEntity.Type.Should().Be(EventType.Shared);
        eventEntity.Participants.Should().Contain(participant);
        eventEntity.CreatorId.Should().Be(creatorId);
    }

    [Fact]
    public void Create_PastDate_ThrowsArgumentException()
    {
        var creatorId = Guid.NewGuid();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        var action = () => Event.Create(
            "Test",
            "Description",
            pastDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be in the past*");
    }

    [Fact]
    public void Create_EmptyCreatorId_ThrowsArgumentException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        var action = () => Event.Create(
            "Test",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            Guid.Empty
        );

        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Update_ValidData_UpdatesEvent()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var newDate = DateTime.UtcNow.AddDays(2);

        var eventEntity = Event.Create(
            "Original Name",
            "Original Description",
            futureDate,
            "Original Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.Update(
            "Updated Name",
            "Updated Description",
            newDate,
            "Updated Location",
            EventType.Shared
        );

        eventEntity.Name.Value.Should().Be("Updated Name");
        eventEntity.Description.Value.Should().Be("Updated Description");
        eventEntity.Location.Value.Should().Be("Updated Location");
        eventEntity.Type.Should().Be(EventType.Shared);
        eventEntity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_PastDate_ThrowsArgumentException()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var pastDate = DateTime.UtcNow.AddDays(-1);

        var eventEntity = Event.Create(
            "Test",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        var action = () => eventEntity.Update(
            "Name",
            "Description",
            pastDate,
            "Location",
            EventType.Exclusive
        );

        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be in the past*");
    }

    [Fact]
    public void AddParticipant_ValidParticipant_AddsToCollection()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Shared Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var participant = User.CreateWithId(participantId, "Participant", "participant@test.com", "hash");

        eventEntity.AddParticipant(participant);

        eventEntity.Participants.Should().Contain(participant);
    }

    [Fact]
    public void AddParticipant_NullParticipant_ThrowsArgumentNullException()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var action = () => eventEntity.AddParticipant(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveParticipant_ValidParticipant_RemovesFromCollection()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Shared Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var participant = User.CreateWithId(participantId, "Participant", "participant@test.com", "hash");
        eventEntity.AddParticipant(participant);

        eventEntity.RemoveParticipant(participant);

        eventEntity.Participants.Should().NotContain(participant);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.Deactivate();

        eventEntity.IsActive.Should().BeFalse();
        eventEntity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.Deactivate();
        eventEntity.Activate();

        eventEntity.IsActive.Should().BeTrue();
        eventEntity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void CanUserView_Creator_ReturnsTrue()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.CanUserView(creatorId).Should().BeTrue();
    }

    [Fact]
    public void CanUserView_SharedEventParticipant_ReturnsTrue()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Shared Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var participant = User.CreateWithId(participantId, "Participant", "participant@test.com", "hash");
        eventEntity.AddParticipant(participant);

        eventEntity.CanUserView(participantId).Should().BeTrue();
    }

    [Fact]
    public void CanUserView_ExclusiveEventNonCreator_ReturnsFalse()
    {
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Exclusive Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.CanUserView(otherUserId).Should().BeFalse();
    }

    [Fact]
    public void CanUserEdit_Creator_ReturnsTrue()
    {
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.CanUserEdit(creatorId).Should().BeTrue();
    }

    [Fact]
    public void CanUserEdit_NonCreator_ReturnsFalse()
    {
        var creatorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Event",
            "Description",
            futureDate,
            "Location",
            EventType.Exclusive,
            creatorId
        );

        eventEntity.CanUserEdit(otherUserId).Should().BeFalse();
    }

    [Fact]
    public void CanUserEdit_SharedEventParticipant_ReturnsFalse()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var eventEntity = Event.Create(
            "Shared Event",
            "Description",
            futureDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        var participant = User.CreateWithId(participantId, "Participant", "participant@test.com", "hash");
        eventEntity.AddParticipant(participant);

        eventEntity.CanUserEdit(participantId).Should().BeFalse();
    }
}
