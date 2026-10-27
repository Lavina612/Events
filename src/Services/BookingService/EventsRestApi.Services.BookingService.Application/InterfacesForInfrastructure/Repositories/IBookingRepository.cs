using EventsRestApi.Services.BookingService.Domain.Models.Entities;

namespace EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.Repositories
{
    public interface IBookingRepository
    {
        Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<Booking>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Guid>> GetIdsByStatusAsync(BookingStatus status, int count, CancellationToken cancellationToken);
        Task<Booking> AddAsync(Guid eventId, Guid userId, int requestedSeats, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Booking newBooking, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task SaveContextAsync(CancellationToken cancellationToken);
        Task<int> GetActiveBookingsCountAsync(Guid id, CancellationToken cancellationToken);
    }
}
