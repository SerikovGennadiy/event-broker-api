using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Events.Application.Common.DTO;
using Events.Application.Common.RequestSpecification;
using Events.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;

namespace Events.API;


[ApiController]
[Authorize]
[Route("events")]
public class EventController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllEvents([FromQuery] EventParameters eventParameters)
    {
        var result = await eventService.GetAllEventsAsync(eventParameters);

        Response.Headers.Append("X-Pagination",
               JsonSerializer.Serialize(result.pageData));

        return Ok(result.eventDTOs);
    }

    [HttpGet("{id:guid}", Name = "EventById")]
    [Authorize]
    public async Task<IActionResult> GetEvent(Guid id)
    {
        var eventDTO = await eventService.GetEventByIdAsync(id);
        return Ok(eventDTO);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEvent eventDTO, CancellationToken stoppingToken = default)
    {
        var _event = await eventService.CreateEventAsync(eventDTO, stoppingToken);
        return CreatedAtRoute(routeName: "EventById", new { id = _event.Id }, _event);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEvent([FromRoute] Guid id, [FromBody] EventDTO eventDTO)
    {
        await eventService.UpdateEventAsync(id, eventDTO);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken stoppingToken = default)
    {
        await eventService.DeleteEventAsync(id, stoppingToken);
        return Ok();
    }
}