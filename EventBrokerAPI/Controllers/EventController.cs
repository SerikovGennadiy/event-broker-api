using Contracts.Service;
using Microsoft.AspNetCore.Mvc;
using Service;
using Shared.DTO;
using Shared.RequestSpecification;
using System.Text.Json;

namespace EventBrokerAPI.Controllers;

[ApiController]
[Route("api/events")]
public class EventController(IServiceManager services): ControllerBase
{
    [HttpGet]
    public IActionResult GetAllEvents([FromQuery] EventParameters eventParameters)
    {
        var result = services.EventService.GetAllEvents(eventParameters);

        Response.Headers.Append("X-Pagination",
               JsonSerializer.Serialize(result.pageData));

        return Ok(result.eventDTOs);
    }

    [HttpGet("{id:guid}", Name="EventById")]
    public IActionResult GetEvent(Guid id)
    {
        var eventDTO = services.EventService.GetEventById(id);
        return Ok(eventDTO);
    }

    [HttpPost]
    [ValidateDTOFilter]
    public IActionResult CreateEvent([FromBody] EventDTO eventDTO)
    {
        var _event = services.EventService.CreateEvent(eventDTO);
        return CreatedAtRoute(routeName: "EventById", new { id = _event.Id }, _event);
    }

    [HttpPost("{eventId}/book")]
    public async Task<IActionResult> CreateEventBooking(Guid eventId, CancellationToken token)
    {
        var bookingDTO = await services.BookingService.CreateBookingAsync(eventId, token);

        return AcceptedAtRoute(
            routeName: "BookingById",
            routeValues: new { eventId = eventId, bookingId = bookingDTO.Id },
            value: bookingDTO
        );
    }

    [HttpPut("{id:guid}")]
    [ValidateDTOFilter]
    public IActionResult UpdateEvent([FromRoute] Guid id, [FromBody] EventDTO eventDTO)
    {
        services.EventService.UpdateEvent(id, eventDTO);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteEvent(Guid id)
    {
        services.EventService.DeleteEvent(id);
        return Ok();
    }
}
