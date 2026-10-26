namespace EventsRestApi.Services.BookingService.Application.Settings
{
    public class BookingBackgroundServiceSettings
    {
        public const string SectionName = "BookingBackgroundServiceSettings";

        public int BunchCount { get; init; } = 5;

        public int DelayMilliseconds { get; init; } = 2000;
    }
}
