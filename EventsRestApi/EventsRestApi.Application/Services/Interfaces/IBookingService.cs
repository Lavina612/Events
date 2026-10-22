using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);

        Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, int requestedSeats, CancellationToken cancellationToken);
    }
}
