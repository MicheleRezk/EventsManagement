using EventManagement.Application.Features.Events.Handlers;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Infrastructure.Data;
using EventManagement.UnitTests.Helpers;
using FluentAssertions;

namespace EventManagement.UnitTests.Events;

public class GetEventByIdQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetEventByIdQueryHandler _handler;

    public GetEventByIdQueryHandlerTests()
    {
        _context = TestDataHelper.CreateInMemoryDbContext();
        _handler = new GetEventByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Given_ExistingEventId_When_GettingEventById_Then_ReturnsEventDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(eventId, userId);
        testEvent.Name = "Specific Event";
        testEvent.Description = "Specific Description";
        testEvent.Location = "Specific Location";

        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var query = new GetEventByIdQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(eventId);
        result.Name.Should().Be("Specific Event");
        result.Description.Should().Be("Specific Description");
        result.Location.Should().Be("Specific Location");
        result.StartTime.Should().Be(testEvent.StartTime);
        result.EndTime.Should().Be(testEvent.EndTime);
        result.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task Given_NonExistingEventId_When_GettingEventById_Then_ReturnsNull()
    {
        // Arrange
        var nonExistingEventId = Guid.NewGuid();
        var query = new GetEventByIdQuery(nonExistingEventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_MultipleEventsInDatabase_When_GettingSpecificEventById_Then_ReturnsOnlyRequestedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetEventId = Guid.NewGuid();

        var event1 = TestDataHelper.CreateTestEvent(createdByUserId: userId);
        event1.Name = "Event 1";

        var targetEvent = TestDataHelper.CreateTestEvent(targetEventId, userId);
        targetEvent.Name = "Target Event";

        var event3 = TestDataHelper.CreateTestEvent(createdByUserId: userId);
        event3.Name = "Event 3";

        _context.Events.AddRange(event1, targetEvent, event3);
        await _context.SaveChangesAsync();

        var query = new GetEventByIdQuery(targetEventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(targetEventId);
        result.Name.Should().Be("Target Event");
        result.CreatedByUserId.Should().Be(userId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
