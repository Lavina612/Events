using EventsRestApi.Models;

namespace EventsRestApi.Services.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);

        Task<Booking> CreateBookingAsync(Guid eventId, int requestedSeats, CancellationToken cancellationToken);
    }
}
