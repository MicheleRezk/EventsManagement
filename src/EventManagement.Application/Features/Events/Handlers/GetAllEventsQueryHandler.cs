using EventManagement.Application.Features.Events.DTOs;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Events.Handlers;

public class GetAllEventsQueryHandler : IRequestHandler<GetAllEventsQuery, IEnumerable<EventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventDto>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .OrderBy(e => e.StartTime)
            .ToListAsync(cancellationToken);

        return events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description,
            e.Location,
            e.StartTime,
            e.EndTime,
            e.CreatedByUserId));
    }
}
