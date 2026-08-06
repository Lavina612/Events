using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> GetBookingByIdAsync(Guid bookingId);

        Task<Booking> CreateBookingAsync(Guid eventId);
    }
}
