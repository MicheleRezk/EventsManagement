using EventManagement.Application.Features.Events.Commands;
using EventManagement.Application.Features.Events.DTOs;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using MediatR;

namespace EventManagement.Application.Features.Events.Handlers;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateEventCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated to create events.");

        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedByUserId = _currentUserService.UserId.Value
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);

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
