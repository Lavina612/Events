using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingRepository
    {
        List<Booking> Get();
        Booking? GetById(Guid id);
        List<Booking> GetByStatus(BookingStatus status, int count);
        Booking Add(Guid eventId);
        bool Update(Booking newBooking);
        bool Delete(Guid id);
    }
}
