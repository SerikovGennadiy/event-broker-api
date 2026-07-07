using Microsoft.AspNetCore.Mvc;

namespace EventBrokerAPI.Controllers;

[ApiController]
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
}
