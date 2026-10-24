using EventsRestApi.Application.Dto.Response;
using EventsRestApi.Application.Mappers;
using EventsRestApi.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventsRestApi.Presentation.Controllers
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
            var foundBooking = await _bookingService.GetBookingByIdAsync(id, CurrentUserId, cancellationToken);

            return Ok(Mapper.MapToBookingResponseDto(foundBooking));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BookingResponseDto>>> GetAllByUserId(CancellationToken cancellationToken = default)
        {
            var bookings = await _bookingService.GetAllByUserIdAsync(CurrentUserId, cancellationToken);

            return Ok(bookings.Select(Mapper.MapToBookingResponseDto).ToList());
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
            await _bookingService.CancelBookingAsync(id, CurrentUserId, cancellationToken);

            return NoContent();
        }
    }
}
