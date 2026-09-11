using EventsRestApi.Data.Repositories.Interfaces;
using EventsRestApi.Exceptions;
using EventsRestApi.Models;
using EventsRestApi.Services.Interfaces;

namespace EventsRestApi.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

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

        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId, int requestedSeats, CancellationToken cancellationToken)
        {
            Booking? addedBooking = null;

            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var foundEvent = await _eventService.GetByIdAsync(eventId, cancellationToken);

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

                addedBooking = await _bookingRepository.AddAsync(eventId, requestedSeats, cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return addedBooking;
        }

        public async Task<int> ProcessPendingBookingsBunchAsync(int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pendingBookings = await _bookingRepository.GetByStatusAsync(BookingStatus.Pending, count, cancellationToken);

            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, cancellationToken));

            await Task.WhenAll(tasks);

            return pendingBookings.Count;
        }

        private async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), _timeProvider, cancellationToken);

            Event? foundEvent = null;

            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                foundEvent = await _eventService.GetByIdAsync(booking.EventId, cancellationToken);

                if (foundEvent == null)
                {
                    booking.Reject(_timeProvider.GetUtcNow().UtcDateTime);
                    await _bookingRepository.SaveContextAsync(cancellationToken);

                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: события с Id: {EventId} не существует.",
                        booking.ProcessedAt,
                        booking.Id,
                        booking.EventId);

                    return;
                }

                if (!foundEvent.IsStillActual(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    booking.Reject(_timeProvider.GetUtcNow().UtcDateTime);
                    foundEvent.ReleaseSeats(booking.BookedSeats);
                    await _bookingRepository.SaveContextAsync(cancellationToken);

                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: события с Id: {EventId} уже завершилось. " +
                        "Количество доступных мест обратно увеличено на {BookingSeats}",
                        booking.ProcessedAt,
                        booking.Id,
                        booking.EventId,
                        booking.BookedSeats);

                    return;
                }

                booking.Confirm(_timeProvider.GetUtcNow().UtcDateTime);
                await _bookingRepository.SaveContextAsync(cancellationToken);

                _logger.LogInformation(
                    "{ProcessedAt}: Бронирование с Id: {BookingId} обработано: {Status}.",
                    booking.ProcessedAt,
                    booking.Id,
                    booking.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: неизвестная ошибка.",
                    booking.ProcessedAt,
                    booking.Id);

                if (foundEvent != null)
                {
                    foundEvent.ReleaseSeats(booking.BookedSeats);
                    await _bookingRepository.SaveContextAsync(cancellationToken);
                }

                booking.Reject(_timeProvider.GetUtcNow().UtcDateTime);
                await _bookingRepository.SaveContextAsync(cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
