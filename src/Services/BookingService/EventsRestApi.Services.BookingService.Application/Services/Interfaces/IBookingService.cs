using EventsRestApi.Services.BookingService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Models;

namespace EventsRestApi.Services.BookingService.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<Booking>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, UserRole userRole, CancellationToken cancellationToken);
        Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, int requestedSeats, CancellationToken cancellationToken);
        Task CancelBookingAsync(Guid bookingId, Guid userId, UserRole userRole, CancellationToken cancellationToken);
    }
}
