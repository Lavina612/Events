using EventsRestApi.Application.Services;
using EventsRestApi.Application.Services.Interfaces;
using EventsRestApi.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsRestApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BookingBackgroundServiceSettings>(
                configuration.GetSection(BookingBackgroundServiceSettings.SectionName));

            services.AddHostedService<BookingBackgroundService>();

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IPasswordService, PasswordService>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
