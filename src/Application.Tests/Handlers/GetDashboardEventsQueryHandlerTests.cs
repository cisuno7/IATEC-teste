using AgendaManager.Application.Handlers.Events;
using AgendaManager.Application.Interfaces;
using AgendaManager.Application.Queries.Events;
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.Enums;
using AgendaManager.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AgendaManager.Application.Tests.Handlers;

public class GetDashboardEventsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly GetDashboardEventsQueryHandler _handler;

    public GetDashboardEventsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _unitOfWorkMock.Setup(u => u.Events).Returns(_eventRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _handler = new GetDashboardEventsQueryHandler(_unitOfWorkMock.Object, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_FiltersBySearchText_ReturnsMatchingEvents()
    {
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);

        var user = User.CreateWithId(userId, "User", "user@test.com", "hash");
        var event1 = Event.Create("Meeting", "Description", futureDate, "Office", EventType.Exclusive, creatorId);
        var event2 = Event.Create("Conference", "Description", futureDate, "Hall", EventType.Exclusive, creatorId);

        var query = new GetDashboardEventsQuery(userId, searchText: "Meeting");

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                userId, null, null, "Meeting", true))
            .ReturnsAsync(new List<Event> { event1 });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Meeting");
    }

    [Fact]
    public async Task Handle_FiltersByDateRange_ReturnsEventsInRange()
    {
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(7);
        var eventDate = DateTime.UtcNow.AddDays(3);

        var user = User.CreateWithId(userId, "User", "user@test.com", "hash");
        var eventEntity = Event.Create("Event", "Description", eventDate, "Location", EventType.Exclusive, creatorId);

        var query = new GetDashboardEventsQuery(userId, startDate: startDate, endDate: endDate);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _dateTimeProviderMock.Setup(p => p.ToUtc(startDate))
            .Returns(startDate.ToUniversalTime());

        _dateTimeProviderMock.Setup(p => p.ToUtc(endDate))
            .Returns(endDate.ToUniversalTime());

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null, true))
            .ReturnsAsync(new List<Event> { eventEntity });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EndDateBeforeStartDate_ThrowsInvalidDateRangeException()
    {
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(7);
        var endDate = DateTime.UtcNow.AddDays(1);
        var normalizedStartDate = startDate.ToUniversalTime();
        var normalizedEndDate = endDate.ToUniversalTime();

        var user = User.CreateWithId(userId, "User", "user@test.com", "hash");

        var query = new GetDashboardEventsQuery(userId, startDate: startDate, endDate: endDate);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _dateTimeProviderMock.Setup(p => p.ToUtc(It.IsAny<DateTime?>()))
            .Returns<DateTime?>(d => d.HasValue ? d.Value.ToUniversalTime() : null);

        _dateTimeProviderMock.Setup(p => p.ToUtc(It.IsAny<DateTime>()))
            .Returns<DateTime>(d => d.ToUniversalTime());

        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<Application.Exceptions.InvalidDateRangeException>()
            .WithMessage("End date cannot be earlier than start date");
    }

    [Fact]
    public async Task Handle_FiltersByPeriodTypeToday_ReturnsTodayEvents()
    {
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var eventDate = now.AddHours(1);

        var user = User.CreateWithId(userId, "User", "user@test.com", "hash");
        var eventEntity = Event.Create("Today Event", "Description", eventDate, "Location", EventType.Exclusive, creatorId);

        var query = new GetDashboardEventsQuery(userId, periodType: "today");

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _dateTimeProviderMock.Setup(p => p.UtcNow)
            .Returns(now);

        _dateTimeProviderMock.Setup(p => p.ToUtcDateOnly(now))
            .Returns(today);

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null, true))
            .ReturnsAsync(new List<Event> { eventEntity });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_FiltersByPeriodTypeWeek_ReturnsWeekEvents()
    {
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var eventDate = today.AddDays(2);

        var user = User.CreateWithId(userId, "User", "user@test.com", "hash");
        var eventEntity = Event.Create("Week Event", "Description", eventDate, "Location", EventType.Exclusive, creatorId);

        var query = new GetDashboardEventsQuery(userId, periodType: "week");

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _dateTimeProviderMock.Setup(p => p.UtcNow)
            .Returns(now);

        _dateTimeProviderMock.Setup(p => p.ToUtcDateOnly(now))
            .Returns(today);

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null, true))
            .ReturnsAsync(new List<Event> { eventEntity });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_FiltersByPeriodTypeMonth_ReturnsMonthEvents()
    {
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var eventDate = today.AddDays(10);

        var user = User.CreateWithId(userId, "User", "user@test.com", "hash");
        var eventEntity = Event.Create("Month Event", "Description", eventDate, "Location", EventType.Exclusive, creatorId);

        var query = new GetDashboardEventsQuery(userId, periodType: "month");

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _dateTimeProviderMock.Setup(p => p.UtcNow)
            .Returns(now);

        _dateTimeProviderMock.Setup(p => p.ToUtcDateOnly(now))
            .Returns(today);

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null, true))
            .ReturnsAsync(new List<Event> { eventEntity });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_CombinesSearchTextAndPeriod_ReturnsFilteredEvents()
    {
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var eventDate = now.AddHours(1);

        var user = User.CreateWithId(userId, "User", "user@test.com", "hash");
        var eventEntity = Event.Create("Meeting", "Description", eventDate, "Location", EventType.Exclusive, creatorId);

        var query = new GetDashboardEventsQuery(userId, periodType: "today", searchText: "Meeting");

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _dateTimeProviderMock.Setup(p => p.UtcNow)
            .Returns(now);

        _dateTimeProviderMock.Setup(p => p.ToUtcDateOnly(now))
            .Returns(today);

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), "Meeting", true))
            .ReturnsAsync(new List<Event> { eventEntity });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_SharedEventAppearsInParticipantDashboard()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var eventDate = today.AddDays(1);

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        var participant = User.CreateWithId(participantId, "Participant", "participant@test.com", "hash");

        var sharedEvent = Event.Create(
            "Shared Event",
            "Description",
            eventDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        sharedEvent.AddParticipant(participant);

        var query = new GetDashboardEventsQuery(participantId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(participantId))
            .ReturnsAsync(participant);

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                participantId, null, null, null, true))
            .ReturnsAsync(new List<Event> { sharedEvent });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Shared Event");
        result.First().Type.Should().Be(EventType.Shared);
        result.First().CreatorId.Should().Be(creatorId);
    }

    [Fact]
    public async Task Handle_SharedEventAppearsInCreatorDashboard()
    {
        var creatorId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var eventDate = today.AddDays(1);

        var creator = User.CreateWithId(creatorId, "Creator", "creator@test.com", "hash");
        var participant = User.CreateWithId(participantId, "Participant", "participant@test.com", "hash");

        var sharedEvent = Event.Create(
            "Shared Event",
            "Description",
            eventDate,
            "Location",
            EventType.Shared,
            creatorId
        );

        sharedEvent.AddParticipant(participant);

        var query = new GetDashboardEventsQuery(creatorId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(creatorId))
            .ReturnsAsync(creator);

        _eventRepositoryMock.Setup(r => r.GetFilteredEventsAsync(
                creatorId, null, null, null, true))
            .ReturnsAsync(new List<Event> { sharedEvent });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Shared Event");
        result.First().CreatorId.Should().Be(creatorId);
    }
}
