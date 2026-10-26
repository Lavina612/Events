namespace EventsRestApi.Services.UserService.IntegrationTests
{
    [CollectionDefinition("PostgreSQL Database Collection")]
    public class DatabaseCollection : ICollectionFixture<DbFixture>
    {
    }
}
