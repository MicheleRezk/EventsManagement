using EventManagement.Application.Features.Events.Handlers;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Infrastructure.Data;
using EventManagement.UnitTests.Helpers;
using FluentAssertions;

namespace EventManagement.UnitTests.Events;

public class GetAllEventsQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetAllEventsQueryHandler _handler;

    public GetAllEventsQueryHandlerTests()
    {
        _context = TestDataHelper.CreateInMemoryDbContext();
        _handler = new GetAllEventsQueryHandler(_context);
    }

    [Fact]
    public async Task Given_MultipleEventsInDatabase_When_GettingAllEvents_Then_ReturnsAllEventsOrderedByStartTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var event1 = TestDataHelper.CreateTestEvent(createdByUserId: userId);
        event1.Name = "Event 1";
        event1.StartTime = DateTime.UtcNow.AddDays(2);
        event1.EndTime = DateTime.UtcNow.AddDays(3);

        var event2 = TestDataHelper.CreateTestEvent(createdByUserId: userId);
        event2.Name = "Event 2";
        event2.StartTime = DateTime.UtcNow.AddDays(1);
        event2.EndTime = DateTime.UtcNow.AddDays(2);

        var event3 = TestDataHelper.CreateTestEvent(createdByUserId: userId);
        event3.Name = "Event 3";
        event3.StartTime = DateTime.UtcNow.AddDays(3);
        event3.EndTime = DateTime.UtcNow.AddDays(4);

        _context.Events.AddRange(event1, event2, event3);
        await _context.SaveChangesAsync();

        var query = new GetAllEventsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        var eventsList = result.ToList();
        eventsList[0].Name.Should().Be("Event 2"); // Earliest start time
        eventsList[1].Name.Should().Be("Event 1");
        eventsList[2].Name.Should().Be("Event 3"); // Latest start time

        // Verify all properties are mapped correctly
        var firstEvent = eventsList.First(e => e.Name == "Event 1");
        firstEvent.Id.Should().Be(event1.Id);
        firstEvent.Description.Should().Be(event1.Description);
        firstEvent.Location.Should().Be(event1.Location);
        firstEvent.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task Given_NoEventsInDatabase_When_GettingAllEvents_Then_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new GetAllEventsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_SingleEventInDatabase_When_GettingAllEvents_Then_ReturnsSingleEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(createdByUserId: userId);
        testEvent.Name = "Single Event";

        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var query = new GetAllEventsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var eventDto = result.First();
        eventDto.Id.Should().Be(testEvent.Id);
        eventDto.Name.Should().Be("Single Event");
        eventDto.Description.Should().Be(testEvent.Description);
        eventDto.Location.Should().Be(testEvent.Location);
        eventDto.CreatedByUserId.Should().Be(userId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
