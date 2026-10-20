using EventsRestApi.Presentation.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.IntegrationsTests
{
    [Collection("PostgreSQL Database Collection")]
    public abstract class BaseDbTest
    {
        private readonly DbFixture _fixture;

        protected BaseDbTest(DbFixture fixture)
        {
            _fixture = fixture;
        }

        protected AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_fixture.Postgres.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new AppDbContext(options);
        }

        protected async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE bookings, events RESTART IDENTITY CASCADE");
        }
    }
}
