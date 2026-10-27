namespace EventsRestApi.Services.EventService.IntegrationTests
{
    [CollectionDefinition("PostgreSQL Database Collection")]
    public class DatabaseCollection : ICollectionFixture<DbFixture>
    {
    }
}
