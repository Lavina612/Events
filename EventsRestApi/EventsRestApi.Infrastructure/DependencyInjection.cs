using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Application.InterfacesForInfrastructure.Services;
using EventsRestApi.Infrastructure.Data;
using EventsRestApi.Infrastructure.Data.Repositories;
using EventsRestApi.Infrastructure.Services;
using EventsRestApi.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsRestApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString("Default"))
                    .UseSnakeCaseNamingConvention());

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
