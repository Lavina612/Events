namespace EventsRestApi.Infrastructure.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Secret { get; set; }

        public int LifetimeInMinutes { get; set; }
    }
}
