using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        
        private readonly IEventService _eventService;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventService eventService)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
        }

        public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId)
        {
            var foundBooking = _bookingRepository.GetById(bookingId);

            return foundBooking == null
                ? null
                : Mapper.MapToBookingResponseDto(foundBooking);
        }

        public async Task<BookingResponseDto?> CreateBookingAsync(Guid eventId)
        {
            if (!_eventService.CanCreateBooking(eventId))
            {
                return null;
            }

            var addedBooking = _bookingRepository.Add(eventId);

            return Mapper.MapToBookingResponseDto(addedBooking);
        }

        public async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            var pendingBookings = _bookingRepository.GetByStatus(BookingStatus.Pending);

            foreach(var booking in pendingBookings)
            {
                await Task.Delay(2000, cancellationToken);

                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;
            }
        }
    }
}
