using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<Booking>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken);
        Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, int requestedSeats, CancellationToken cancellationToken);
        Task CancelBookingAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken);
    }
}
