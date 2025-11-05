using EventManagement.Application.Features.Events.DTOs;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Events.Handlers;

public class
    GetEventsCreatedByUserQueryHandler : IRequestHandler<GetEventsCreatedByUserQuery, IEnumerable<DetailedEventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetEventsCreatedByUserQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<DetailedEventDto>> Handle(GetEventsCreatedByUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated to view their events.");

        var events = await _context.Events
            .Where(e => e.CreatedByUserId == _currentUserService.UserId.Value)
            .Include(e => e.Registrations)
            .OrderBy(e => e.StartTime)
            .ToListAsync(cancellationToken);

        return events.Select(e => new DetailedEventDto(
            e.Id,
            e.Name,
            e.Description,
            e.Location,
            e.StartTime,
            e.EndTime,
            e.Registrations.Select(r => new RegistrationDto(
                r.Id,
                r.Name,
                r.PhoneNumber,
                r.Email,
                r.CreatedAt)
            )));
    }
}