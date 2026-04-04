using Contracts.Service;
using EventBrokerAPI.Controllers.ActionFilter;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO;

namespace EventBrokerAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventController(IServiceManager services): ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllEvents()
        {
            var eventDTOs = services.EventService.GetAllEvents();
            return Ok(eventDTOs);
        }

        [HttpGet("{id:guid}", Name="EventById")]
        public IActionResult GetEvent(Guid id)
        {
            var eventDTO = services.EventService.GetEventById(id);
            return Ok(eventDTO);
        }

        [HttpPost]
        [ServiceFilter(typeof(DTOValidationAttribute))]
        public IActionResult CreateEvent([FromBody] EventDTO eventDTO)
        {
            var _event = services.EventService.CreateEvent(eventDTO);
            return CreatedAtRoute(routeName: "EventById", new { id = _event.Id }, _event);
        }

        [HttpPut("{id: guid}")]
        [ServiceFilter(typeof(DTOValidationAttribute))]
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
}
