using EventManagement.Application.DTOs;
using EventManagement.Application.Exceptions;
using EventManagement.Application.Features.Events.Commands;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Events.Handlers;

public class RegisterForEventCommandHandler : IRequestHandler<RegisterForEventCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;

    public RegisterForEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegistrationDto> Handle(RegisterForEventCommand request, CancellationToken cancellationToken)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == request.EventId, cancellationToken);

        if (!eventExists)
            throw new NotFoundException($"Event with ID {request.EventId} not found.");

        var existingRegistration = await _context.Registrations
            .AnyAsync(r =>
                r.EventId == request.EventId && r.Email == request.Email, cancellationToken);

        if (existingRegistration)
            throw new InvalidOperationException($"Email {request.Email} is already registered for this event.");

        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email
        };

        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegistrationDto(
            registration.Id,
            registration.EventId,
            registration.Name,
            registration.PhoneNumber,
            registration.Email);
    }
}