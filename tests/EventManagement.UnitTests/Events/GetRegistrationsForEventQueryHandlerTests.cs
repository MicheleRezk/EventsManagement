using EventManagement.Application.Exceptions;
using EventManagement.Application.Features.Events.Handlers;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Data;
using EventManagement.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace EventManagement.UnitTests.Events;

public class GetRegistrationsForEventQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly GetRegistrationsForEventQueryHandler _handler;
    private readonly Guid _testUserId;

    public GetRegistrationsForEventQueryHandlerTests()
    {
        _context = TestDataHelper.CreateInMemoryDbContext();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new GetRegistrationsForEventQueryHandler(_context, _mockCurrentUserService.Object);

        _testUserId = Guid.NewGuid();
        var testUser = TestDataHelper.CreateTestUser(_testUserId);
        _context.Users.Add(testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Given_EventCreatorAndExistingRegistrations_When_GettingRegistrations_Then_ReturnsRegistrations()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        var eventId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(eventId, _testUserId);
        _context.Events.Add(testEvent);

        var registration1 = TestDataHelper.CreateTestRegistration(eventId: eventId);
        registration1.Name = "Charlie Brown";
        registration1.Email = "charlie@example.com";

        var registration2 = TestDataHelper.CreateTestRegistration(eventId: eventId);
        registration2.Name = "Michele George";
        registration2.Email = "michele.george@example.com";

        var registration3 = TestDataHelper.CreateTestRegistration(eventId: eventId);
        registration3.Name = "Bob Smith";
        registration3.Email = "bob@example.com";

        _context.Registrations.AddRange(registration1, registration2, registration3);
        await _context.SaveChangesAsync();

        var query = new GetRegistrationsForEventQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        var registrationsList = result.ToList();
        registrationsList.Should().ContainSingle(r => r.Name == registration1.Name);
        registrationsList.Should().ContainSingle(r => r.Name == registration2.Name);
        registrationsList.Should().ContainSingle(r => r.Name == registration3.Name);
        
        var aliceRegistration = registrationsList.First(r => r.Name == registration2.Name);
        aliceRegistration.Id.Should().Be(registration2.Id);
        aliceRegistration.Name.Should().Be(registration2.Name);
        aliceRegistration.Email.Should().Be(registration2.Email);
    }

    [Fact]
    public async Task Given_UnauthenticatedUser_When_GettingRegistrations_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);
        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);

        var eventId = Guid.NewGuid();
        var query = new GetRegistrationsForEventQuery(eventId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("User must be authenticated to view registrations.");
    }
    

    [Fact]
    public async Task Given_NonExistingEventId_When_GettingRegistrations_Then_ThrowsNotFoundException()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        var nonExistingEventId = Guid.NewGuid();
        var query = new GetRegistrationsForEventQuery(nonExistingEventId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage($"Event with ID {nonExistingEventId} not found.");
    }

    [Fact]
    public async Task Given_EventNotCreatedByCurrentUser_When_GettingRegistrations_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        var otherUserId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(eventId, otherUserId); // Created by other user
        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var query = new GetRegistrationsForEventQuery(eventId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("You can only view registrations for events you created.");
    }

    [Fact]
    public async Task Given_EventCreatorWithNoRegistrations_When_GettingRegistrations_Then_ReturnsEmptyCollection()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        var eventId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(eventId, _testUserId);
        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var query = new GetRegistrationsForEventQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_EventCreatorAndRegistrationsForMultipleEvents_When_GettingRegistrations_Then_ReturnsOnlyRegistrationsForSpecifiedEvent()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_testUserId);

        var targetEventId = Guid.NewGuid();
        var otherEventId = Guid.NewGuid();

        var targetEvent = TestDataHelper.CreateTestEvent(targetEventId, _testUserId);
        var otherEvent = TestDataHelper.CreateTestEvent(otherEventId, _testUserId);
        _context.Events.AddRange(targetEvent, otherEvent);
        
        var targetRegistration = TestDataHelper.CreateTestRegistration(eventId: targetEventId);
        targetRegistration.Name = "Target Registrant";
        
        var otherRegistration = TestDataHelper.CreateTestRegistration(eventId: otherEventId);
        otherRegistration.Name = "Other Registrant";

        var expectedRegisterdAt = DateTime.UtcNow;
        _context.Registrations.AddRange(targetRegistration, otherRegistration);
        await _context.SaveChangesAsync();

        var query = new GetRegistrationsForEventQuery(targetEventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be(targetRegistration.Name);
        result.First().RegisteredAt.Should().BeCloseTo(expectedRegisterdAt, TimeSpan.FromSeconds(1));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
