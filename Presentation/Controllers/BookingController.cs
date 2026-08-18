using Application.Common.DTO;
using Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBrokerAPI.Controllers;

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
}
