using EventsRestApi.Dto.Response;
using EventsRestApi.Exceptions;
using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

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

            await _semaphoreSlim.WaitAsync();
            try
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

                addedBooking = _bookingRepository.Add(eventId, requestedSeats);
                _eventService.Update(foundEvent);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return Mapper.MapToBookingResponseDto(addedBooking);
        }

        public async Task<int> ProcessPendingBookingsBunchAsync(int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pendingBookings = _bookingRepository.GetByStatus(BookingStatus.Pending, count);

            var tasks = pendingBookings.Select(booking =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return ProcessBookingAsync(booking, cancellationToken);
            });

            await Task.WhenAll(tasks);

            return pendingBookings.Count;
        }

        private async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), _timeProvider, cancellationToken);

            Event? foundEvent = null;

            await _semaphoreSlim.WaitAsync();
            try
            {
                foundEvent = _eventService.GetById(booking.EventId);

                if (foundEvent == null)
                {
                    booking.Status = BookingStatus.Rejected;
                    booking.ProcessedAt = _timeProvider.GetUtcNow().UtcDateTime;

                    _bookingRepository.Update(booking);

                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: события с Id: {EventId} не существует.",
                        booking.ProcessedAt,
                        booking.Id,
                        booking.EventId);

                    throw new NotFoundEventException(booking.EventId);
                }

                if (!foundEvent.IsStillActual(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    booking.Status = BookingStatus.Rejected;
                    booking.ProcessedAt = _timeProvider.GetUtcNow().UtcDateTime;

                    foundEvent.ReleaseSeats(booking.BookedSeats);

                    _eventService.Update(foundEvent);
                    _bookingRepository.Update(booking);

                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: события с Id: {EventId} уже завершилось. " +
                        "Количество доступных мест обратно увеличено на {BookingSeats}",
                        booking.ProcessedAt,
                        booking.Id,
                        booking.EventId,
                        booking.BookedSeats);

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
            catch
            {
                if (foundEvent != null)
                {
                    foundEvent.ReleaseSeats(booking.BookedSeats);

                    _eventService.Update(foundEvent);
                }

                booking.Status = BookingStatus.Rejected;
                booking.ProcessedAt = _timeProvider.GetUtcNow().UtcDateTime;

                _bookingRepository.Update(booking);

                _logger.LogWarning(
                    "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: неизвестная ошибка.",
                    booking.ProcessedAt,
                    booking.Id);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
