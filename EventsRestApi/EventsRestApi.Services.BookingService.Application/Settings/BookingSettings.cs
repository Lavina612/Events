namespace EventsRestApi.Services.BookingService.Application.Settings
{
    public class BookingSettings
    {
        public const string SectionName = "BookingSettings";

        public int ActiveBookingLimit { get; set; } = 10;
    }
}
