using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IBookingRepository
    {
        IReadOnlyList<Booking> Get();
        Booking? GetById(Guid id);
        IReadOnlyList<Booking> GetByStatus(BookingStatus status, int count);
        Booking Add(Guid eventId, int requestedSeats);
        bool Update(Booking newBooking);
        bool Delete(Guid id);
    }
}
