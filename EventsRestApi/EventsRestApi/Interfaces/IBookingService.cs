using EventsRestApi.Dto.Response;

namespace EventsRestApi.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);

        Task<BookingResponseDto> CreateBookingAsync(Guid eventId, int requestedSeats, CancellationToken cancellationToken);

        Task<int> ProcessPendingBookingsBunchAsync(int count, CancellationToken cancellationToken);
    }
}
