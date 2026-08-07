using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        private readonly IBookingService _bookingService;

        public EventsController(
            IEventService eventService,
            IBookingService bookingService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public ActionResult<PaginatedResult<EventResponseDto>> GetAll(
            [FromQuery] string? title, 
            [FromQuery] DateTime? from, 
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            return _eventService.Get(title, from, to, page, pageSize);
        }

        [HttpGet("{id:guid}")]
        public ActionResult<EventResponseDto> GetById(Guid id)
        {
            var foundEventDto = _eventService.GetById(id);

            if (foundEventDto == null)
            {
                return NotFound($"Событие с Id: {id} не найдено.");
            }

            return foundEventDto;
        }

        [HttpPost]
        public IActionResult Add(EventRequestDto addingEventDto)
        {
            var addedEventDto = _eventService.Add(addingEventDto);

            return CreatedAtAction(nameof(GetById), new { id = addedEventDto.Id }, addedEventDto);
        }

        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, EventRequestDto updatingEventDto)
        {
            var isUpdated = _eventService.Update(id, updatingEventDto);

            if (!isUpdated)
            {
                return NotFound($"Событие с Id: {id} не найдено.");
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var isDeleted = _eventService.Delete(id);

            if (!isDeleted)
            {
                return NotFound($"Событие с Id: {id} не найдено.");
            }

            return NoContent();
        }

        [HttpPost("{eventId:guid}/book")]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(Guid eventId, CancellationToken cancellationToken)
        {
            var createdBooking = await _bookingService.CreateBookingAsync(eventId, cancellationToken);

            return createdBooking == null
                ? NotFound($"Невозможно создать бронь, т.к. событие с Id: {eventId} не найдено либо уже завершилось.")
                : AcceptedAtRoute(
                    routeName: "GetBookingById",
                    routeValues: new { id = createdBooking.Id },
                    value: new { 
                        Message = $"Создана бронь для события.", 
                        Id = createdBooking.Id,
                        EventId = eventId,
                        Status = createdBooking.Status
                    });
        }
    }
}
