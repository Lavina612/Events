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

        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventService eventService,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
            _logger = logger;
        }

        public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var foundBooking = _bookingRepository.GetById(bookingId);

            return foundBooking == null
                ? null
                : Mapper.MapToBookingResponseDto(foundBooking);
        }

        public async Task<BookingResponseDto?> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
        {
            if (!_eventService.CanCreateBooking(eventId))
            {
                return null;
            }

            var addedBooking = _bookingRepository.Add(eventId);

            return Mapper.MapToBookingResponseDto(addedBooking);
        }

        public async Task<int> ProcessPendingBookingsBunchAsync(int count, CancellationToken cancellationToken)
        {
            var pendingBookings = _bookingRepository.GetByStatus(BookingStatus.Pending, count);

            foreach(var booking in pendingBookings)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(2000, cancellationToken);

                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;

                _bookingRepository.Update(booking);

                _logger.LogInformation("Бронирование с Id: {BookingId} подтверждено.", booking.Id);
            }

            return pendingBookings.Count;
        }
    }
}
