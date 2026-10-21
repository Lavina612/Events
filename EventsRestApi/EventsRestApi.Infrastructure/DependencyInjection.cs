using EventsRestApi.Application.RepositoryInterfaces;
using EventsRestApi.Infrastructure.Data;
using EventsRestApi.Infrastructure.Data.Repositories;
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

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            return services;
        }
    }
}
