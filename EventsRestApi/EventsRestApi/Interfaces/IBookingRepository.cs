using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingRepository
    {
        List<Booking> Get();
        Booking? GetById(Guid id);
        Booking Add(Booking addingBooking);
        bool Update(Guid id, Booking newBooking);
        bool Delete(Guid id);
    }
}
