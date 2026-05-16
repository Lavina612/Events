using EventsRestApi.Dto;
using EventsRestApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController(IEventService eventService) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;

        [HttpGet]
        public ActionResult<List<EventDto>> GetAll()
        {
            return _eventService.GetAll();
        }

        [HttpGet("{id}")]
        public ActionResult<EventDto> GetById(int id)
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
        public IActionResult Add(EventDto addingEvent)
        {
            if (addingEvent.StartAt >= addingEvent.EndAt)
            {
                return BadRequest("Дата окончания должна быть позже даты начала.");
            }

            _eventService.Add(addingEvent);

            return CreatedAtAction(nameof(GetById), new { id = addingEvent.Id }, addingEvent);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, EventDto updatingEvent)
        {
            if (updatingEvent.StartAt >= updatingEvent.EndAt)
            {
                return BadRequest("Дата окончания должна быть позже даты начала.");
            }

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
