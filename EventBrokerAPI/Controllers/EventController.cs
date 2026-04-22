using Contracts.Service;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO;
using Shared.RequestSpecification;

namespace EventBrokerAPI.Controllers;

[ApiController]
[Route("api/events")]
public class EventController(IServiceManager services): ControllerBase
{
    [HttpGet]
    public IActionResult GetAllEvents([FromQuery] EventParameters eventParameters)
    {
        var eventDTOs = services.EventService.GetAllEvents(eventParameters);
        return Ok(eventDTOs);
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
