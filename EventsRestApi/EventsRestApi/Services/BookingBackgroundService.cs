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
            _logger.LogInformation("{ServiceName} запущен.", nameof(BookingBackgroundService));

            var bunchCount = 5;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    
                    var processedBookingsCount = await bookingService.ProcessPendingBookingsBunchAsync(bunchCount, cancellationToken);

                    if (processedBookingsCount < bunchCount)
                    {
                        await Task.Delay(2000, cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирования.");
                    await Task.Delay(2000, cancellationToken);
                }
            }

            _logger.LogInformation("{ServiceName} остановлен.", nameof(BookingBackgroundService));
        }
    }
}
