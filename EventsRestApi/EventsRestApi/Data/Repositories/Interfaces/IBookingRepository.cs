using EventsRestApi.Presentation.Models.Entities;

namespace EventsRestApi.Presentation.Data.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken);
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Guid>> GetIdsByStatusAsync(BookingStatus status, int count, CancellationToken cancellationToken);
        Task<Booking> AddAsync(Guid eventId, int requestedSeats, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Booking newBooking, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task SaveContextAsync(CancellationToken cancellationToken);
    }
}
