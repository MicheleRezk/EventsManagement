namespace EventManagement.Application.DTOs;

public record CreateEventDto(
    string Name,
    string Description,
    string Location,
    DateTime StartTime,
    DateTime EndTime);
