using EventsRestApi.Dto.Response;
using EventsRestApi.Exceptions;
using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class BookingService : IBookingService
    {
        private static readonly Lock _bookingLock = new();

        private readonly IBookingRepository _bookingRepository;

        private readonly IEventService _eventService;

        private readonly TimeProvider _timeProvider;

        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventService eventService,
            TimeProvider timeProvider,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var foundBooking = _bookingRepository.GetById(bookingId);

            return foundBooking == null
                ? null
                : Mapper.MapToBookingResponseDto(foundBooking);
        }

        public async Task<BookingResponseDto> CreateBookingAsync(Guid eventId, int requestedSeats, CancellationToken cancellationToken)
        {
            Booking? addedBooking = null;

            lock (_bookingLock)
            {
                var foundEvent = _eventService.GetById(eventId);

                if (foundEvent == null)
                {
                    throw new NotFoundEventException(eventId);
                }

                if (!foundEvent.IsStillActual(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    throw new FinishedEventException(eventId, foundEvent.EndAt);
                }

                if (!foundEvent.TryReserveSeats(requestedSeats))
                {
                    throw new NoAvailableSeatsException(eventId, requestedSeats, foundEvent.AvailableSeats);
                }

                addedBooking = _bookingRepository.Add(eventId);
                _eventService.Update(foundEvent);
            }

            return Mapper.MapToBookingResponseDto(addedBooking);
        }

        public async Task<int> ProcessPendingBookingsBunchAsync(int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pendingBookings = _bookingRepository.GetByStatus(BookingStatus.Pending, count);

            foreach (var booking in pendingBookings)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(2), _timeProvider, cancellationToken);

                var foundEvent = _eventService.GetById(booking.EventId);

                if (foundEvent == null)
                {
                    booking.Status = BookingStatus.Rejected;
                    throw new NotFoundEventException(booking.EventId);
                }

                if (!foundEvent.IsStillActual(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    booking.Status = BookingStatus.Rejected;
                    throw new FinishedEventException(foundEvent.Id, foundEvent.EndAt);
                }

                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = _timeProvider.GetUtcNow().UtcDateTime;

                _bookingRepository.Update(booking);

                _logger.LogInformation(
                    "{ProcessedAt}: Бронирование с Id: {BookingId} обработано: {Status}.",
                    booking.ProcessedAt,
                    booking.Id,
                    booking.Status);
            }

            return pendingBookings.Count;
        }
    }
}
