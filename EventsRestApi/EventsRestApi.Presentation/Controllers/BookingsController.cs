using EventsRestApi.Application.Dto.Response;
using EventsRestApi.Application.Mappers;
using EventsRestApi.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventsRestApi.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        private Guid CurrentUserId
        {
            get
            {
                var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimValue) || !Guid.TryParse(claimValue, out var userId))
                {
                    throw new UnauthorizedAccessException("Идентификатор пользователя не найден в токене.");
                }

                return userId;
            }
        }

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
            var bookings = await _bookingService.GetAllByUserId(CurrentUserId, cancellationToken);

            return Ok(bookings.Select(Mapper.MapToBookingResponseDto).ToList());
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await _bookingService.DeleteBookingAsync(id, CurrentUserId, cancellationToken);

            return NoContent();
        }

        [HttpPost("{id:guid}/cancel")]
        [Authorize(Roles = "Admin")]
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
