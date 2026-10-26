using EventsRestApi.Services.UserService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.UserService.Application.InterfacesForInfrastructure.Services;
using EventsRestApi.Services.UserService.Infrastructure.Data;
using EventsRestApi.Services.UserService.Infrastructure.Data.Repositories;
using EventsRestApi.Services.UserService.Infrastructure.Services;
using EventsRestApi.Services.UserService.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventsRestApi.Services.UserService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserDbContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString("Default"))
                    .UseSnakeCaseNamingConvention());

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<ITokenService, TokenService>();

            return services;
        }

        public static async Task ApplyMigrationsAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                await db.Database.MigrateAsync();
            }
        }
    }
}
