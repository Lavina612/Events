using EventsRestApi.Application.Dto.Response;
using EventsRestApi.Application.Mappers;
using EventsRestApi.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Presentation.Controllers
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var foundBooking = await _bookingService.GetBookingByIdAsync(id, cancellationToken);

            if (foundBooking == null)
            {
                return Problem(
                    detail: $"Бронь с Id: {id} не найдена.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Mapper.MapToBookingResponseDto(foundBooking);
        }
    }
}
