namespace EventManagement.Application.Features.Events.DTOs;

public record CreateEventDto(
    string Name,
    string Description,
    string Location,
    DateTime StartTime,
    DateTime EndTime);
