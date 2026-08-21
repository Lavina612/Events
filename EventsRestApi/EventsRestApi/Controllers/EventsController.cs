using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
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
            var resultWithEvents = _eventService.Get(title, from, to, page, pageSize);

            return new PaginatedResult<EventResponseDto>(
                resultWithEvents.ItemsForPage.Select(Mapper.MapToEventResponseDto).ToList(),
                resultWithEvents.TotalCount,
                resultWithEvents.Page,
                resultWithEvents.PageSize,
                resultWithEvents.TotalPages);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<EventResponseDto> GetById(Guid id)
        {
            var foundEvent = _eventService.GetById(id);

            if (foundEvent == null)
            {
                return Problem(
                    detail: $"Событие с Id: {id} не найдено.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Mapper.MapToEventResponseDto(foundEvent);
        }

        [HttpPost]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult Add(EventRequestDto addingEventDto)
        {
            var addedEvent = _eventService.Add(Mapper.MapToEvent(addingEventDto));

            return CreatedAtAction(nameof(GetById), new { id = addedEvent.Id }, Mapper.MapToEventResponseDto(addedEvent));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult Update(Guid id, EventRequestDto updatingEventDto)
        {
            var isUpdated = _eventService.Update(Mapper.MapToEvent(id, updatingEventDto));

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
            [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Количество бронируемых мест должно быть положительным.")] int requestedSeats,
            CancellationToken cancellationToken)
        {
            var createdBooking = await _bookingService.CreateBookingAsync(eventId, requestedSeats, cancellationToken);

            return AcceptedAtAction(
                actionName: "GetById",
                controllerName: "Bookings",
                routeValues: new { id = createdBooking.Id },
                value: Mapper.MapToBookingResponseDto(createdBooking));
        }
    }
}
