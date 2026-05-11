using Contracts.Service;
using Microsoft.AspNetCore.Mvc;

namespace EventBrokerAPI.Controllers;

[ApiController]
[Route("api/events/{eventId}/bookings")]
public class BookingController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    public BookingController(IServiceManager serviceManager) => _serviceManager = serviceManager;

    [HttpGet("{bookingId}", Name = "BookingById")]
    public async Task<IActionResult> GetBooking(Guid bookingId, CancellationToken token)
    {
        var booking = await _serviceManager.BookingService.GetBookingByIdAsync(bookingId, token);
        return Ok(booking);
    }
}
