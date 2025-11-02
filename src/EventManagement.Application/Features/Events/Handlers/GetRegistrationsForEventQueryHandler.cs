using EventManagement.Application.DTOs;
using EventManagement.Application.Exceptions;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Events.Handlers;

public class GetRegistrationsForEventQueryHandler : IRequestHandler<GetRegistrationsForEventQuery, IEnumerable<RegistrationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetRegistrationsForEventQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<RegistrationDto>> Handle(GetRegistrationsForEventQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated to view registrations.");
        
        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

        if (eventEntity == null)
            throw new NotFoundException($"Event with ID {request.EventId} not found.");

        if (eventEntity.CreatedByUserId != _currentUserService.UserId.Value)
            throw new UnauthorizedAccessException("You can only view registrations for events you created.");

        var registrations = await _context.Registrations
            .Where(r => r.EventId == request.EventId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return registrations.Select(r => new RegistrationDto(
            r.Id,
            r.EventId,
            r.Name,
            r.PhoneNumber,
            r.Email));
    }
}
