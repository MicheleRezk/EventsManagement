using EventManagement.Application.Features.Events.DTOs;
using EventManagement.Application.Features.Events.Queries;
using EventManagement.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Features.Events.Handlers;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto?>
{
    private readonly IApplicationDbContext _context;

    public GetEventByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (eventEntity == null)
            return null;

        return new EventDto(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Description,
            eventEntity.Location,
            eventEntity.StartTime,
            eventEntity.EndTime,
            eventEntity.CreatedByUserId);
    }
}
