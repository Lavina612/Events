using EventsRestApi.Services.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services.UserService.IntegrationTests
{
    [Collection("PostgreSQL Database Collection")]
    public abstract class BaseDbTest
    {
        private readonly DbFixture _fixture;

        protected BaseDbTest(DbFixture fixture)
        {
            _fixture = fixture;
        }

        protected UserDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseNpgsql(_fixture.Postgres.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new UserDbContext(options);
        }

        protected async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE users RESTART IDENTITY CASCADE");
        }
    }
}
