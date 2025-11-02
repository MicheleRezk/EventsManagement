using EventManagement.Application.DTOs;
using MediatR;

namespace EventManagement.Application.Features.Events.Queries;

public record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;
