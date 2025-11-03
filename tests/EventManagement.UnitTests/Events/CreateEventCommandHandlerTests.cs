using EventManagement.Application.Features.Events.Commands;
using EventManagement.Application.Features.Events.Handlers;
using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Data;
using EventManagement.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EventManagement.UnitTests.Events;

public class CreateEventCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CreateEventCommandHandler _handler;
    private readonly Guid _testUserId;

    public CreateEventCommandHandlerTests()
    {
        _context = TestDataHelper.CreateInMemoryDbContext();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new CreateEventCommandHandler(_context, _mockCurrentUserService.Object);

        // Seed test user
        _testUserId = Guid.NewGuid();
        var testUser = TestDataHelper.CreateTestUser(_testUserId);
        _context.Users.Add(testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Given_AuthenticatedUserAndValidCommand_When_CreatingEvent_Then_EventIsPersistedAndReturned()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        var command = new CreateEventCommand(
            "Tech Conference 2024",
            "Annual technology conference",
            "Convention Center",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Location.Should().Be(command.Location);
        result.StartTime.Should().Be(command.StartTime);
        result.EndTime.Should().Be(command.EndTime);
        result.CreatedByUserId.Should().Be(_testUserId);
        result.Id.Should().NotBeEmpty();

        var savedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == result.Id);
        savedEvent.Should().NotBeNull();
        savedEvent!.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task Given_UnauthenticatedUser_When_CreatingEvent_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);
        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new CreateEventCommand(
            "Unauthorized Event",
            "This should fail",
            "Nowhere",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("User must be authenticated to create events.");

        var eventCount = await _context.Events.CountAsync();
        eventCount.Should().Be(0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
