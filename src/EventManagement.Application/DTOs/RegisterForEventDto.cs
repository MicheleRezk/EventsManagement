namespace EventManagement.Application.DTOs;

public record RegisterForEventDto(
    string Name,
    string PhoneNumber,
    string Email);
