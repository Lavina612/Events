using EventsRestApi.Dto.Response;
using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);

        Task<BookingResponseDto?> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken);

        Task<int> ProcessPendingBookingsBunchAsync(int count, CancellationToken cancellationToken);
    }
}
