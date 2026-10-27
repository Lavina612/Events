using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Cache;
using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.EventService.Infrastructure.Data;
using EventsRestApi.Services.EventService.Infrastructure.Data.Repositories;
using EventsRestApi.Services.EventService.Infrastructure.MessageBroker;
using EventsRestApi.Services.EventService.Infrastructure.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

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

            var redisConnectionString = configuration.GetConnectionString("Redis");

            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Строка подключения к Redis обязательно должна присутствовать в конфигурационном файле.");
            }

            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<IEventCacheService, RedisEventCacheService>();

            services.AddHostedService<KafkaInitializerHost>();
            services.AddHostedService<KafkaBookingEventConsumer>();

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
