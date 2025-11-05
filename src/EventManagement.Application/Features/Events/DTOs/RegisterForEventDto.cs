namespace EventManagement.Application.Features.Events.DTOs;

public record RegisterForEventDto(
    string Name,
    string PhoneNumber,
    string Email);
