using EventsRestApi.Services.BookingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventsRestApi.Services.BookingService.IntegrationTests
{
    public class DbFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder("postgres:16-alpine").Build();

        public async Task InitializeAsync()
        {
            await Postgres.StartAsync();

            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseNpgsql(Postgres.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .Options;

            await using var context = new BookingDbContext(options);
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Postgres.DisposeAsync();
        }
    }
}
