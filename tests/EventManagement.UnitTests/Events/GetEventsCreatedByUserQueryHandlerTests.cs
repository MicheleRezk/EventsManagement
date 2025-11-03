using EventManagement.Application.Features.Events.Handlers;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Data;
using EventManagement.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace EventManagement.UnitTests.Events;

public class GetEventsCreatedByUserQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly GetEventsCreatedByUserQueryHandler _handler;
    private readonly Guid _testUserId;

    public GetEventsCreatedByUserQueryHandlerTests()
    {
        _context = TestDataHelper.CreateInMemoryDbContext();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new GetEventsCreatedByUserQueryHandler(_context, _mockCurrentUserService.Object);

        _testUserId = Guid.NewGuid();
        var testUser = TestDataHelper.CreateTestUser(_testUserId);
        _context.Users.Add(testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Given_AuthenticatedUserWithEvents_When_GettingEventsCreatedByUser_Then_ReturnsOnlyUserEvents()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        var otherUserId = Guid.NewGuid();

        // User's events
        var userEvent1 = TestDataHelper.CreateTestEvent(createdByUserId: _testUserId);
        userEvent1.Name = "User Event 1";
        userEvent1.StartTime = DateTime.UtcNow.AddDays(2);

        var userEvent2 = TestDataHelper.CreateTestEvent(createdByUserId: _testUserId);
        userEvent2.Name = "User Event 2";
        userEvent2.StartTime = DateTime.UtcNow.AddDays(1);
        
        var otherUserEvent = TestDataHelper.CreateTestEvent(createdByUserId: otherUserId);
        otherUserEvent.Name = "Other User Event";

        _context.Events.AddRange(userEvent1, userEvent2, otherUserEvent);
        await _context.SaveChangesAsync();

        var query = new GetEventsCreatedByUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.CreatedByUserId == _testUserId);

        var eventsList = result.ToList();
        eventsList[0].Name.Should().Be("User Event 2");
        eventsList[1].Name.Should().Be("User Event 1");
    }

    [Fact]
    public async Task Given_UnauthenticatedUser_When_GettingEventsCreatedByUser_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);
        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);

        var query = new GetEventsCreatedByUserQuery();

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("User must be authenticated to view their events.");
    }
    
    [Fact]
    public async Task Given_AuthenticatedUserWithNoEvents_When_GettingEventsCreatedByUser_Then_ReturnsEmptyCollection()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        // Add events for other users only
        var otherUserId = Guid.NewGuid();
        var otherUserEvent = TestDataHelper.CreateTestEvent(createdByUserId: otherUserId);
        _context.Events.Add(otherUserEvent);
        await _context.SaveChangesAsync();

        var query = new GetEventsCreatedByUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
