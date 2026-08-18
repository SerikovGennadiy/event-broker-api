using Application.Common.DTO;
using Application.Common.RequestSpecification;
using Application.Contracts.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EventBrokerAPI.Controllers;

[ApiController]
[Authorize]
[Route("events")]
public class EventController(IEventService eventService, IBookingService bookingService): ControllerBase
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
    [ValidateDTOFilter]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEvent eventDTO)
    {
        var _event = await eventService.CreateEventAsync(eventDTO);
        return CreatedAtRoute(routeName: "EventById", new { id = _event.Id }, _event);
    }

    [HttpPost("{eventId}/book")]
    [Authorize]
    [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEventBooking(Guid eventId, CancellationToken token)
    {
        var bookingDTO = await bookingService.CreateBookingAsync(eventId);

        return AcceptedAtRoute(
            routeName: "BookingById",
            routeValues: new { bookingId = bookingDTO.Id },
            value: bookingDTO
        );
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ValidateDTOFilter]
    public async Task<IActionResult> UpdateEvent([FromRoute] Guid id, [FromBody] EventDTO eventDTO)
    {
        await eventService.UpdateEventAsync(id, eventDTO);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        await eventService.DeleteEventAsync(id);
        return Ok();
    }
}
