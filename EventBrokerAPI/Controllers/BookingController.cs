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
    public IActionResult GetBooking(Guid bookingId)
    {
        var booking = _serviceManager.BookingService.GetBookingByIdAsync(bookingId);
        return Ok(booking);
    }
}
