using EventManagement.Application.DTOs;
using MediatR;

namespace EventManagement.Application.Features.Events.Commands;

public record CreateEventCommand(
    string Name,
    string Description,
    string Location,
    DateTime StartTime,
    DateTime EndTime) : IRequest<EventDto>;
