using EventManagement.Application.DTOs;
using MediatR;

namespace EventManagement.Application.Features.Events.Queries;

public record GetRegistrationsForEventQuery(Guid EventId) : IRequest<IEnumerable<RegistrationDto>>;
