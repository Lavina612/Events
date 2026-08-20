using EventsRestApi.Interfaces;
using EventsRestApi.Settings;
using Microsoft.Extensions.Options;

namespace EventsRestApi.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly ILogger<BookingBackgroundService> _logger;

        private readonly IBookingService _bookingService;

        private readonly BookingBackgroundServiceSettings _settings;

        public BookingBackgroundService(
            ILogger<BookingBackgroundService> logger,
            IBookingService bookingService,
            IOptions<BookingBackgroundServiceSettings> settings)
        {
            _logger = logger;
            _bookingService = bookingService;
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
                    var processedBookingsCount = await _bookingService.ProcessPendingBookingsBunchAsync(_settings.BunchCount, cancellationToken);

                    if (processedBookingsCount < _settings.BunchCount)
                    {
                        await Task.Delay(_settings.DelayMilliseconds, cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирования.");
                    await Task.Delay(_settings.DelayMilliseconds, cancellationToken);
                }
            }

            _logger.LogInformation("{ServiceName} остановлен.", nameof(BookingBackgroundService));
        }
    }
}
