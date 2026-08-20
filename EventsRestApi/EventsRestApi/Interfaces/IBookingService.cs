using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);

        Task<Booking> CreateBookingAsync(Guid eventId, int requestedSeats, CancellationToken cancellationToken);

        Task<int> ProcessPendingBookingsBunchAsync(int count, CancellationToken cancellationToken);
    }
}
