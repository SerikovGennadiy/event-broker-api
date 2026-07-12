using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace EventBrokerAPI.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Check()
    {
        var canConnect = _context.Database.CanConnect();
        return canConnect
            ? Ok("Connected to database")
            : StatusCode(500, "Cannot connect to database");
    }
}