namespace EventsRestApi.Application.Settings
{
    public class BookingBackgroundServiceSettings
    {
        public const string SectionName = "BookingBackgroundServiceSettings";

        public int BunchCount { get; set; } = 5;

        public int DelayMilliseconds { get; set; } = 2000;
    }
}
