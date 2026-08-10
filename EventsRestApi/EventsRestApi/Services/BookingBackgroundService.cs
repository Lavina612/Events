using EventsRestApi.Interfaces;
using EventsRestApi.Settings;
using Microsoft.Extensions.Options;

namespace EventsRestApi.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly ILogger<BookingBackgroundService> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly BookingBackgroundServiceSettings _settings;

        public BookingBackgroundService(
            ILogger<BookingBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<BookingBackgroundServiceSettings> settings)
        {
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
                    using var scope = _scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    var processedBookingsCount = await bookingService.ProcessPendingBookingsBunchAsync(_settings.BunchCount, cancellationToken);

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
