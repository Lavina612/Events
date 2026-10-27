namespace EventsRestApi.Services.BookingService.IntegrationTests
{
    [CollectionDefinition("PostgreSQL Database Collection")]
    public class DatabaseCollection : ICollectionFixture<DbFixture>
    {
    }
}
