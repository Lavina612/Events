using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var foundBookingDto = await _bookingService.GetBookingByIdAsync(id, cancellationToken);

            if (foundBookingDto == null)
            {
                return Problem(
                    detail: $"Бронь с Id: {id} не найдена.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return foundBookingDto;
        }
    }
}
