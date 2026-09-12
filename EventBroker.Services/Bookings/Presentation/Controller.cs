using Bookings.Application.Common.DTO;
using Bookings.Application.Contracts.Services;
using Bookings.Application.Services;
using Bookings.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookings.API;

[ApiController]
[Authorize]
[Route("bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingController(IBookingService service) => _service = service;

    [HttpGet("{bookingId}", Name = "BookingById")]
    public async Task<IActionResult> GetBooking(Guid bookingId, CancellationToken token = default)
    {
        var booking = await _service.GetBookingByIdAsync(bookingId);
        return Ok(booking);
    }

    [HttpDelete("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBooking(Guid bookingId, CancellationToken token = default)
    {
        await _service.CancelBookingAsync(bookingId);
        return NoContent();
    }

    [HttpPost("{eventId}/book")]
    [Authorize]
    [ProducesResponseType(typeof(BookingDTO), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEventBooking(Guid eventId, CancellationToken token)
    {
        var bookingDTO = await _service.CreateBookingAsync(eventId, token);

        return AcceptedAtRoute(
            routeName: "BookingById",
            routeValues: new { bookingId = bookingDTO.Id },
            value: bookingDTO
        );
    }
}
