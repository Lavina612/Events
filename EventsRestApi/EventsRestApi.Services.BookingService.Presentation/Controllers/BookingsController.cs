using EventsRestApi.Services.BookingService.Application.Dto.Response;
using EventsRestApi.Services.BookingService.Application.Mappers;
using EventsRestApi.Services.BookingService.Application.Services.Interfaces;
using EventsRestApi.Services.SharedContracts.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventsRestApi.Services.BookingService.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class BookingsController : BaseApiController
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var foundBooking = await _bookingService.GetBookingByIdAsync(id, CurrentUserId, CurrentUserRole, cancellationToken);

            return Ok(BookingMapper.MapToBookingResponseDto(foundBooking));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BookingResponseDto>>> GetAllByUserId(CancellationToken cancellationToken = default)
        {
            var bookings = await _bookingService.GetAllByUserIdAsync(CurrentUserId, cancellationToken);

            return Ok(bookings.Select(BookingMapper.MapToBookingResponseDto).ToList());
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken = default)
        {
            await _bookingService.CancelBookingAsync(id, CurrentUserId, CurrentUserRole, cancellationToken);

            return NoContent();
        }

        [HttpPost("/events/{eventId:guid}/book")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(
            Guid eventId,
            [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Количество бронируемых мест должно быть положительным.")] int requestedSeats,
            CancellationToken cancellationToken = default)
        {
            var createdBooking = await _bookingService.CreateBookingAsync(eventId, CurrentUserId, requestedSeats, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = createdBooking.Id }, BookingMapper.MapToBookingResponseDto(createdBooking));
        }
    }
}
