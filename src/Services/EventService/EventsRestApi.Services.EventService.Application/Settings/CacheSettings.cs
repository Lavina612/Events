namespace EventsRestApi.Services.EventService.Application.Settings
{
    public class CacheSettings
    {
        public const string SectionName = "CacheSettings";
        public int EventByIdTtlMinutes { get; set; } = 5;
        public int TopEventsTtlMinutes { get; set; } = 10;
    }
}
