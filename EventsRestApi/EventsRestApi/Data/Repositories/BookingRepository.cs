using EventsRestApi.Data.Repositories.Interfaces;
using EventsRestApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Data.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        private readonly TimeProvider _timeProvider;

        public BookingRepository(
            AppDbContext context,
            TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken)
        {
            var bookings = await _context.Bookings.ToListAsync(cancellationToken);
            return bookings.AsReadOnly();
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetIdsByStatusAsync(BookingStatus status, int count, CancellationToken cancellationToken)
        {
            var bookingIdsByStatus = await _context.Bookings
                .Where(x => x.Status == status)
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.Id)
                .Take(count)
                .ToListAsync(cancellationToken);

            return bookingIdsByStatus.AsReadOnly();
        }

        public async Task<Booking> AddAsync(Guid eventId, int requestedSeats, CancellationToken cancellationToken)
        {
            var addingBooking = new Booking(
                Guid.NewGuid(),
                eventId,
                BookingStatus.Pending,
                requestedSeats,
                _timeProvider.GetUtcNow().UtcDateTime);

            await _context.Bookings.AddAsync(addingBooking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return addingBooking;
        }

        public async Task<bool> UpdateAsync(Booking newBooking, CancellationToken cancellationToken)
        {
            var updatingBooking = await GetByIdAsync(newBooking.Id, cancellationToken);

            if (updatingBooking == null)
            {
                return false;
            }

            updatingBooking.ChangeStatus(newBooking.Status, newBooking.ProcessedAt);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var deletingBooking = await GetByIdAsync(id, cancellationToken);

            if (deletingBooking == null)
            {
                return false;
            }

            _context.Bookings.Remove(deletingBooking);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task SaveContextAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
