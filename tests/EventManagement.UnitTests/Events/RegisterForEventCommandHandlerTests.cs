using EventManagement.Application.Exceptions;
using EventManagement.Application.Features.Events.Commands;
using EventManagement.Application.Features.Events.Handlers;
using EventManagement.Infrastructure.Data;
using EventManagement.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.UnitTests.Events;

public class RegisterForEventCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RegisterForEventCommandHandler _handler;

    public RegisterForEventCommandHandlerTests()
    {
        _context = TestDataHelper.CreateInMemoryDbContext();
        _handler = new RegisterForEventCommandHandler(_context);
    }

    [Fact]
    public async Task Given_ValidCommandAndExistingEvent_When_RegisteringForEvent_Then_RegistrationIsCreatedAndReturned()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(eventId);
        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var command = new RegisterForEventCommand(
            eventId,
            "John Doe",
            "+1234567890",
            "john.doe@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.Name.Should().Be("John Doe");
        result.PhoneNumber.Should().Be("+1234567890");
        result.Email.Should().Be("john.doe@example.com");
        result.Id.Should().NotBeEmpty();

        var savedRegistration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.Id == result.Id);
        savedRegistration.Should().NotBeNull();
        savedRegistration!.EventId.Should().Be(eventId);
        savedRegistration.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task Given_NonExistingEventId_When_RegisteringForEvent_Then_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingEventId = Guid.NewGuid();
        var command = new RegisterForEventCommand(
            nonExistingEventId,
            "John Doe",
            "+1234567890",
            "john.doe@example.com");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage($"Event with ID {nonExistingEventId} not found.");

        var registrationCount = await _context.Registrations.CountAsync();
        registrationCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_DuplicateEmailForSameEvent_When_RegisteringForEvent_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(eventId);
        _context.Events.Add(testEvent);

        var existingRegistration = TestDataHelper.CreateTestRegistration(eventId: eventId);
        existingRegistration.Email = "duplicate@example.com";
        _context.Registrations.Add(existingRegistration);

        await _context.SaveChangesAsync();

        var command = new RegisterForEventCommand(
            eventId,
            "Different Name",
            "+0987654321",
            "duplicate@example.com");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Email duplicate@example.com is already registered for this event.");

        var registrationCount = await _context.Registrations.CountAsync();
        registrationCount.Should().Be(1); // Only the original registration should exist
    }

    [Fact]
    public async Task Given_SameEmailForDifferentEvents_When_RegisteringForEvent_Then_RegistrationIsCreated()
    {
        // Arrange
        var event1Id = Guid.NewGuid();
        var event2Id = Guid.NewGuid();
        var testEvent1 = TestDataHelper.CreateTestEvent(event1Id);
        var testEvent2 = TestDataHelper.CreateTestEvent(event2Id);
        _context.Events.AddRange(testEvent1, testEvent2);

        // Register for first event
        var existingRegistration = TestDataHelper.CreateTestRegistration(eventId: event1Id);
        existingRegistration.Email = "same@example.com";
        _context.Registrations.Add(existingRegistration);

        await _context.SaveChangesAsync();

        // Register same email for second event
        var command = new RegisterForEventCommand(
            event2Id,
            "Same Person",
            "+1234567890",
            "same@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(event2Id);
        result.Email.Should().Be("same@example.com");

        var registrationCount = await _context.Registrations.CountAsync();
        registrationCount.Should().Be(2); // Both registrations should exist
    }

    [Fact]
    public async Task Given_ValidCommand_When_RegisteringForEvent_Then_GeneratesUniqueRegistrationId()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var testEvent = TestDataHelper.CreateTestEvent(eventId);
        _context.Events.Add(testEvent);
        await _context.SaveChangesAsync();

        var command1 = new RegisterForEventCommand(
            eventId,
            "Person One",
            "+1111111111",
            "person1@example.com");

        var command2 = new RegisterForEventCommand(
            eventId,
            "Person Two",
            "+2222222222",
            "person2@example.com");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Id.Should().NotBe(result2.Id);
        result1.Id.Should().NotBeEmpty();
        result2.Id.Should().NotBeEmpty();

        var registrationCount = await _context.Registrations.CountAsync();
        registrationCount.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
