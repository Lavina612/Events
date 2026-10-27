using EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.MessageBroker;
using EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.BookingService.Infrastructure.Data;
using EventsRestApi.Services.BookingService.Infrastructure.Data.Repositories;
using EventsRestApi.Services.BookingService.Infrastructure.MessageBroker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventsRestApi.Services.BookingService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BookingDbContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString("Default"))
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

            return services;
        }

        public static async Task ApplyMigrationsAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                await db.Database.MigrateAsync();
            }
        }
    }
}
