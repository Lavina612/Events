using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        [ProducesResponseType(typeof(PaginatedResult<EventResponseDto>), StatusCodes.Status200OK)]
        public ActionResult<PaginatedResult<EventResponseDto>> GetAll(
            [FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть положительным.")] int page = 1,
            [FromQuery][Range(1, 100, ErrorMessage = "Размер страницы должен быть от 1 до 100.")] int pageSize = 10)
        {
            return _eventService.Get(title, from, to, page, pageSize);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<EventResponseDto> GetById(Guid id)
        {
            var foundEventDto = _eventService.GetDtoById(id);

            if (foundEventDto == null)
            {
                return Problem(
                    detail: $"Событие с Id: {id} не найдено.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return foundEventDto;
        }

        [HttpPost]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
        public IActionResult Add(EventRequestDto addingEventDto)
        {
            var addedEventDto = _eventService.Add(addingEventDto);

            return CreatedAtAction(nameof(GetById), new { id = addedEventDto.Id }, addedEventDto);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult Update(Guid id, EventRequestDto updatingEventDto)
        {
            var isUpdated = _eventService.Update(id, updatingEventDto);

            if (!isUpdated)
            {
                return Problem(
                    detail: $"Событие с Id: {id} не найдено.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid id)
        {
            var isDeleted = _eventService.Delete(id);

            if (!isDeleted)
            {
                return Problem(
                    detail: $"Событие с Id: {id} не найдено.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return NoContent();
        }

        [HttpPost("{eventId:guid}/book")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(
            Guid eventId, 
            [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Количество запрашиваемых мест должно быть положительным.")] int requestedSeats, 
            CancellationToken cancellationToken)
        {
            var createdBooking = await _bookingService.CreateBookingAsync(eventId, requestedSeats, cancellationToken);

            return AcceptedAtAction(
                actionName: "GetById",
                controllerName: "Bookings",
                routeValues: new { id = createdBooking.Id },
                value: createdBooking);
        }
    }
}
