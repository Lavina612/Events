namespace EventsRestApi.Application.Settings
{
    public class UserSettings
    {
        public const string SectionName = "UserSettings";

        public int ActiveBookingLimit { get; init; } = 10;
    }
}
