namespace EventManagement.Application.Features.Events.DTOs;

public record RegistrationDto(
    Guid Id,
    Guid EventId,
    string Name,
    string PhoneNumber,
    string Email);
