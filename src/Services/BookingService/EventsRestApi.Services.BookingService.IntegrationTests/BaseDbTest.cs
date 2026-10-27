using EventsRestApi.Services.BookingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services.BookingService.IntegrationTests
{
    [Collection("PostgreSQL Database Collection")]
    public abstract class BaseDbTest
    {
        private readonly DbFixture _fixture;

        protected BaseDbTest(DbFixture fixture)
        {
            _fixture = fixture;
        }

        protected BookingDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseNpgsql(_fixture.Postgres.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new BookingDbContext(options);
        }

        protected async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE bookings RESTART IDENTITY CASCADE");
        }
    }
}
