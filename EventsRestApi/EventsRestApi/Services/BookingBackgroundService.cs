using EventsRestApi.Presentation.Data.Repositories.Interfaces;
using EventsRestApi.Presentation.Models.Entities;
using EventsRestApi.Presentation.Services.Interfaces;
using EventsRestApi.Presentation.Settings;
using Microsoft.Extensions.Options;

namespace EventsRestApi.Presentation.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly TimeProvider _timeProvider;

        private readonly ILogger<BookingBackgroundService> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly BookingBackgroundServiceSettings _settings;

        public BookingBackgroundService(
            TimeProvider timeProvider,
            ILogger<BookingBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<BookingBackgroundServiceSettings> settings)
        {
            _timeProvider = timeProvider;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            _logger.LogInformation("{ServiceName} запущен.", nameof(BookingBackgroundService));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    IReadOnlyList<Guid> pendingBookingIds;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                        pendingBookingIds = await bookingRepository.GetIdsByStatusAsync(BookingStatus.Pending, _settings.BunchCount, cancellationToken);
                    }

                    var tasks = pendingBookingIds.Select(bookingId => ProcessBookingAsync(bookingId, cancellationToken));

                    var tasksResult = await Task.WhenAll(tasks);

                    var processedBookingsCount = tasksResult.Count(x => x);

                    if (processedBookingsCount < _settings.BunchCount)
                    {
                        await Task.Delay(_settings.DelayMilliseconds, cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирования.");
                    await Task.Delay(_settings.DelayMilliseconds, cancellationToken);
                }
            }

            _logger.LogInformation("{ServiceName} остановлен.", nameof(BookingBackgroundService));
        }

        private async Task<bool> ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), _timeProvider, cancellationToken);

            await _semaphoreSlim.WaitAsync(cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            IBookingRepository? bookingRepository = null;
            Event? foundEvent = null;
            Booking? booking = null;

            try
            {
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);

                if (booking == null || booking.Status != BookingStatus.Pending)
                {
                    return false;
                }

                foundEvent = await eventService.GetByIdAsync(booking.EventId, cancellationToken);

                if (foundEvent == null)
                {
                    booking.Reject(_timeProvider.GetUtcNow().UtcDateTime);
                    await bookingRepository.SaveContextAsync(cancellationToken);

                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: события с Id: {EventId} не существует.",
                        booking.ProcessedAt,
                        booking.Id,
                        booking.EventId);

                    return true;
                }

                if (!foundEvent.IsStillActual(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    booking.Reject(_timeProvider.GetUtcNow().UtcDateTime);
                    foundEvent.ReleaseSeats(booking.BookedSeats);
                    await bookingRepository.SaveContextAsync(cancellationToken);

                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: события с Id: {EventId} уже завершилось. " +
                        "Количество доступных мест обратно увеличено на {BookedSeats}",
                        booking.ProcessedAt,
                        booking.Id,
                        booking.EventId,
                        booking.BookedSeats);

                    return true;
                }

                booking.Confirm(_timeProvider.GetUtcNow().UtcDateTime);
                await bookingRepository.SaveContextAsync(cancellationToken);

                _logger.LogInformation(
                    "{ProcessedAt}: Бронирование с Id: {BookingId} обработано: {Status}.",
                    booking.ProcessedAt,
                    booking.Id,
                    booking.Status);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                if (booking == null)
                {
                    _logger.LogInformation(
                        "Обработка бронирования с Id: {BookingId} была отменена.",
                        bookingId);
                }
                else
                {
                    _logger.LogInformation(
                        "{ProcessedAt}: Обработка бронирования с Id: {BookingId} была отменена.",
                        booking.ProcessedAt,
                        booking.Id);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (booking == null)
                {
                    _logger.LogError(
                        ex,
                        "Во время загрузки бронирования с Id: {BookingId} возникла неизвестная ошибка.",
                        bookingId);

                    return false;
                }

                try
                {
                    if (foundEvent != null)
                    {
                        foundEvent.ReleaseSeats(booking.BookedSeats);
                    }

                    booking.Reject(_timeProvider.GetUtcNow().UtcDateTime);
                    await bookingRepository!.SaveContextAsync(cancellationToken);

                    _logger.LogError(
                        ex,
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: неизвестная ошибка.",
                        booking.ProcessedAt,
                        booking.Id);

                    return true;
                }
                catch (Exception innerEx)
                {
                    _logger.LogCritical(
                        innerEx,
                        "Не удалось отклонить бронирование с Id: {BookingId}: неизвестная ошибка.",
                        bookingId);
                }

                return false;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
