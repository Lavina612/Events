using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly List<Booking> _bookings = [];

        private readonly TimeProvider _timeProvider;

        public BookingRepository(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public IReadOnlyList<Booking> Get()
        {
            return _bookings.AsReadOnly();
        }

        public Booking? GetById(Guid id)
        {
            return _bookings.FirstOrDefault(x => x.Id == id);
        }

        public IReadOnlyList<Booking> GetByStatus(BookingStatus status, int count)
        {
            return _bookings
                .Where(x => x.Status == status)
                .OrderBy(x => x.CreatedAt)
                .Take(count)
                .ToList()
                .AsReadOnly();
        }

        public Booking Add(Guid eventId, int requestedSeats)
        {
            var addingBooking = new Booking(
                Guid.NewGuid(),
                eventId,
                BookingStatus.Pending,
                requestedSeats,
                _timeProvider.GetUtcNow().UtcDateTime);

            _bookings.Add(addingBooking);

            return addingBooking;
        }

        public bool Update(Booking newBooking)
        {
            var updatingBooking = GetById(newBooking.Id);

            if (updatingBooking == null)
            {
                return false;
            }

            updatingBooking.Status = newBooking.Status;
            updatingBooking.ProcessedAt = newBooking.ProcessedAt;

            return true;
        }

        public bool Delete(Guid id)
        {
            var deletingBooking = GetById(id);

            if (deletingBooking == null)
            {
                return false;
            }

            _bookings.Remove(deletingBooking);

            return true;
        }
    }
}
