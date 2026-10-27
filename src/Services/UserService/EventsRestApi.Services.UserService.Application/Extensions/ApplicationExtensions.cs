using EventsRestApi.Services.UserService.Application.Services;
using EventsRestApi.Services.UserService.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsRestApi.Services.UserService.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUserService, Services.UserService>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
