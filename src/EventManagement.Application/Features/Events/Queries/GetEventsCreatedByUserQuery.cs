using EventManagement.Application.DTOs;
using MediatR;

namespace EventManagement.Application.Features.Events.Queries;

public record GetEventsCreatedByUserQuery() : IRequest<IEnumerable<DetailedEventDto>>;
