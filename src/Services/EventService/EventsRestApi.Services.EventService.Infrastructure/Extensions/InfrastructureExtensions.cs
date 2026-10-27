using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.EventService.Infrastructure.Data;
using EventsRestApi.Services.EventService.Infrastructure.Data.Repositories;
using EventsRestApi.Services.EventService.Infrastructure.MessageBroker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventsRestApi.Services.EventService.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<EventDbContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString("Default"))
                    .UseSnakeCaseNamingConvention());

            services.AddHostedService<KafkaInitializerHost>();
            services.AddHostedService<BookingEventConsumer>();

            services.AddScoped<IEventRepository, EventRepository>();

            return services;
        }

        public static async Task ApplyMigrationsAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
                await db.Database.MigrateAsync();
            }
        }
    }
}
