using EventsRestApi.Interfaces;
using EventsRestApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController(IEventService eventService) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;

        [HttpGet]
        public ActionResult<List<Event>> GetAll()
        {
            return _eventService.GetAll();
        }

        [HttpGet("{id}")]
        public ActionResult<Event> GetById(int id)
        {
            try
            {
                return _eventService.GetById(id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Add(Event addingEvent)
        {
            _eventService.Add(addingEvent);

            return CreatedAtAction(nameof(GetById), new { id = addingEvent.Id }, addingEvent);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Event updatingEvent)
        {
            try
            {
                _eventService.Update(id, updatingEvent);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                _eventService.Delete(id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
