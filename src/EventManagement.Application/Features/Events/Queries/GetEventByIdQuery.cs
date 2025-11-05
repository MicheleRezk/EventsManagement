using EventManagement.Application.Features.Events.DTOs;
using MediatR;

namespace EventManagement.Application.Features.Events.Queries;

public record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;
