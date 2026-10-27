using EventsRestApi.Services.EventService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services.EventService.IntegrationTests
{
    [Collection("PostgreSQL Database Collection")]
    public abstract class BaseDbTest
    {
        private readonly DbFixture _fixture;

        protected BaseDbTest(DbFixture fixture)
        {
            _fixture = fixture;
        }

        protected EventDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseNpgsql(_fixture.Postgres.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new EventDbContext(options);
        }

        protected async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE events RESTART IDENTITY CASCADE");
        }
    }
}
