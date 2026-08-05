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

        public Booking Add(Booking addingBooking)
        {
            addingBooking.Id = new Guid();
            addingBooking.Status = BookingStatus.Pending;
            addingBooking.CreatedAt = DateTime.UtcNow;

            _bookings.Add(addingBooking);

            return addingBooking;
        }

        public bool Update(Guid id, Booking newBooking)
        {
            var updatingBooking = GetById(id);

            if (updatingBooking == null)
            {
                return false;
            }

            updatingBooking.EventId = newBooking.EventId;
            updatingBooking.Status = newBooking.Status;
            updatingBooking.CreatedAt = newBooking.CreatedAt;
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
