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

        [HttpGet("{id:guid}", Name = "GetBookingById")]
        public async Task<ActionResult<BookingResponseDto>> GetById(Guid id)
        {
            var foundBookingDto = await _bookingService.GetBookingByIdAsync(id);

            if (foundBookingDto == null)
            {
                return NotFound($"Бронь с Id: {id} не найдена.");
            }

            return foundBookingDto;
        }
    }
}
