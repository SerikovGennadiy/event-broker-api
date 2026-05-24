using Contracts.Service;
using Entities.ErrorHandling.Model;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO;
using Shared.RequestSpecification;
using System.Text.Json;

namespace EventBrokerAPI.Controllers;

[ApiController]
[Route("events")]
public class EventController(IEventService eventService, IBookingService bookingService): ControllerBase
{
    [HttpGet]
    public IActionResult GetAllEvents([FromQuery] EventParameters eventParameters)
    {
        var result = eventService.GetAllEvents(eventParameters);

        Response.Headers.Append("X-Pagination",
               JsonSerializer.Serialize(result.pageData));

        return Ok(result.eventDTOs);
    }

    [HttpGet("{id:guid}", Name="EventById")]
    public IActionResult GetEvent(Guid id)
    {
        var eventDTO = eventService.GetEventById(id);
        return Ok(eventDTO);
    }

    [HttpPost]
    [ValidateDTOFilter]
    public IActionResult CreateEvent([FromBody] CreateEvent eventDTO)
    {
        var _event = eventService.CreateEvent(eventDTO);
        return CreatedAtRoute(routeName: "EventById", new { id = _event.Id }, _event);
    }

    [HttpPost("{eventId}/book")]
    [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEventBooking(Guid eventId, CancellationToken token)
    {
        var bookingDTO = await bookingService.CreateBookingAsync(eventId, token);

        return AcceptedAtRoute(
            routeName: "BookingById",
            routeValues: new { bookingId = bookingDTO.Id },
            value: bookingDTO
        );
    }

    [HttpPut("{id:guid}")]
    [ValidateDTOFilter]
    public IActionResult UpdateEvent([FromRoute] Guid id, [FromBody] EventDTO eventDTO)
    {
        eventService.UpdateEvent(id, eventDTO);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteEvent(Guid id)
    {
        eventService.DeleteEvent(id);
        return Ok();
    }
}
