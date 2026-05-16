using EventsRestApi.Dto;
using EventsRestApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public ActionResult<List<EventDto>> GetAll()
        {
            return _eventService.GetAll();
        }

        [HttpGet("{id:int}")]
        public ActionResult<EventDto> GetById(int id)
        {
            var foundEventDto = _eventService.GetById(id);

            if (foundEventDto == null)
            {
                return NotFound($"Событие с Id: {id} не найдено.");
            }

            return foundEventDto;
        }

        [HttpPost]
        public IActionResult Add(EventDto addingEventDto)
        {
            var addedEventDto = _eventService.Add(addingEventDto);

            return CreatedAtAction(nameof(GetById), new { id = addedEventDto.Id }, addedEventDto);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, EventDto updatingEventDto)
        {
            if (id != updatingEventDto.Id)
            {
                return BadRequest("Id в теле запроса должно совпадать с Id в URL.");
            }

            var isUpdated = _eventService.Update(id, updatingEventDto);

            if (!isUpdated)
            {
                return NotFound($"Событие с Id: {id} не найдено.");
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var isDeleted = _eventService.Delete(id);

            if (!isDeleted)
            {
                return NotFound($"Событие с Id: {id} не найдено.");
            }

            return NoContent();
        }
    }
}
