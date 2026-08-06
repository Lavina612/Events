using EventsRestApi.Dto.Response;
using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId);

        Task<BookingResponseDto?> CreateBookingAsync(Guid eventId);
    }
}
