using EventsRestApi.Services.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventsRestApi.Services.UserService.IntegrationTests
{
    public class DbFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder("postgres:16-alpine").Build();

        public async Task InitializeAsync()
        {
            await Postgres.StartAsync();

            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseNpgsql(Postgres.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .Options;

            await using var context = new UserDbContext(options);
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Postgres.DisposeAsync();
        }
    }
}
