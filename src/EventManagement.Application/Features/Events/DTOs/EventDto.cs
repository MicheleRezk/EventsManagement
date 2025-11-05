namespace EventManagement.Application.Features.Events.DTOs;

public record EventDto(
    Guid Id,
    string Name,
    string Description,
    string Location,
    DateTime StartTime,
    DateTime EndTime,
    Guid CreatedByUserId);
