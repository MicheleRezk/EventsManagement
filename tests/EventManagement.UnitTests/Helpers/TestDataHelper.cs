using EventManagement.Domain.Entities;
using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.UnitTests.Helpers;

public static class TestDataHelper
{
    public static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static User CreateTestUser(Guid? userId = null)
    {
        return new User
        {
            Id = userId ?? Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedPassword"
        };
    }

    public static Event CreateTestEvent(Guid? eventId = null, Guid? createdByUserId = null)
    {
        return new Event
        {
            Id = eventId ?? Guid.NewGuid(),
            Name = "Test Event",
            Description = "Test Description",
            Location = "Test Location",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2),
            CreatedByUserId = createdByUserId ?? Guid.NewGuid()
        };
    }

    public static Registration CreateTestRegistration(Guid? registrationId = null, Guid? eventId = null)
    {
        return new Registration
        {
            Id = registrationId ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            Name = "Test Registrant",
            PhoneNumber = "+1234567890",
            Email = "registrant@example.com"
        };
    }
}
