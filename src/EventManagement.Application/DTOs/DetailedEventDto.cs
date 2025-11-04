namespace EventManagement.Application.DTOs;

public record DetailedEventDto(
    Guid Id,
    string Name,
    string Description,
    string Location,
    DateTime StartTime,
    DateTime EndTime,
    IEnumerable<RegistrationDto> Registrations);
