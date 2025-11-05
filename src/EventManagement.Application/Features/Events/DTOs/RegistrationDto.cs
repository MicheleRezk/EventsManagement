namespace EventManagement.Application.Features.Events.DTOs;

public record RegistrationDto(
    Guid Id,
    string Name,
    string PhoneNumber,
    string Email,
    DateTimeOffset RegisteredAt);
