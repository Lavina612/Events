using EventsRestApi.Services.EventService.Application.Services.Interfaces;
using EventsRestApi.Services.EventService.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsRestApi.Services.EventService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));

            services.AddScoped<IEventService, Services.EventService>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
