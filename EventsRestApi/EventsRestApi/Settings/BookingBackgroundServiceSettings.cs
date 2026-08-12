namespace EventsRestApi.Settings
{
    public class BookingBackgroundServiceSettings
    {
        public const string SectionName = "BookingBackgroundService";

        public int BunchCount { get; set; } = 5;

        public int DelayMilliseconds { get; set; } = 2000;
    }
}
