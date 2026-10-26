using EventsRestApi.Services.BookingService.Application.Services;
using EventsRestApi.Services.BookingService.Application.Services.Interfaces;
using EventsRestApi.Services.BookingService.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsRestApi.Services.BookingService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BookingBackgroundServiceSettings>(configuration.GetSection(BookingBackgroundServiceSettings.SectionName));
            services.Configure<BookingSettings>(configuration.GetSection(BookingSettings.SectionName));


            services.AddHostedService<BookingBackgroundService>();

            services.AddScoped<IBookingService, Services.BookingService>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
