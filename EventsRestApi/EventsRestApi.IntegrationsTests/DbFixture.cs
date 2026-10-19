using EventsRestApi.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventsRestApi.IntegrationsTests
{
    public class DbFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder("postgres:16-alpine").Build();

        public async Task InitializeAsync()
        {
            await Postgres.StartAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(Postgres.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .Options;

            await using var context = new AppDbContext(options);
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Postgres.DisposeAsync();
        }
    }
}
