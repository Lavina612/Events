using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingRepository
    {
        List<Booking> Get();
        Booking? GetById(Guid id);
        List<Booking> GetByStatus(BookingStatus status);
        Booking Add(Guid eventId);
        bool Update(Guid id, Booking newBooking);
        bool Delete(Guid id);
    }
}
