using EventsRestApi.Interfaces;

namespace EventsRestApi.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly ILogger<BookingBackgroundService> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        public BookingBackgroundService(
            ILogger<BookingBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(BookingBackgroundService)} запущен.");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    
                    await bookingService.ProcessPendingBookingsAsync(cancellationToken);

                    await Task.Delay(2000, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирования.");
                }
            }

            _logger.LogInformation($"{nameof(BookingBackgroundService)} остановлен.");
        }
    }
}
