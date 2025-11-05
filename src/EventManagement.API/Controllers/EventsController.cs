using EventManagement.Application.Features.Events.Commands;
using EventManagement.Application.Features.Events.DTOs;
using EventManagement.Application.Features.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAllEvents()
    {
        var query = new GetAllEventsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id)
    {
        var query = new GetEventByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{eventId}/register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<RegistrationDto>> RegisterForEvent(Guid eventId, [FromBody] RegisterForEventDto request)
    {
        var command = new RegisterForEventCommand(eventId, request.Name, request.PhoneNumber, request.Email);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEventById), new { id = eventId }, result);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto request)
    {
        var command = new CreateEventCommand(
            request.Name,
            request.Description,
            request.Location,
            request.StartTime,
            request.EndTime);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEventById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpGet("creator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DetailedEventDto>>> GetEventsCreatedByUser()
    {
        var query = new GetEventsCreatedByUserQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{eventId}/registrations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegistrationDto>>> GetRegistrationsForEvent(Guid eventId)
    {
        var query = new GetRegistrationsForEventQuery(eventId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
