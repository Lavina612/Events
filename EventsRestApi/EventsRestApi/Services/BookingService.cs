using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            return _bookingRepository.GetById(bookingId);
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            return _bookingRepository.Add(eventId);
        }
    }
}
