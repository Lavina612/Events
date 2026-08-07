using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private static readonly List<Booking> _bookings = [];

        public List<Booking> Get()
        {
            return _bookings;
        }

        public Booking? GetById(Guid id)
        {
            return _bookings.FirstOrDefault(x => x.Id == id);
        }

        public List<Booking> GetByStatus(BookingStatus status, int count)
        {
            return _bookings.Where(x => x.Status == status).Take(count).ToList();
        }

        public Booking Add(Guid eventId)
        {
            var addingBooking = new Booking(
                Guid.NewGuid(),
                eventId,
                BookingStatus.Pending,
                DateTime.UtcNow);

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
