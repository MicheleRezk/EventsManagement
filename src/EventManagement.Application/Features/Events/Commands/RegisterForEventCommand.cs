using EventManagement.Application.Features.Events.DTOs;
using MediatR;

namespace EventManagement.Application.Features.Events.Commands;

public record RegisterForEventCommand(
    Guid EventId,
    string Name,
    string PhoneNumber,
    string Email) : IRequest<RegistrationDto>;
