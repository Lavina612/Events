using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace EventsRestApi.Services.BookingService.Presentation.Extensions
{
    public static class PresentationExtensions
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var issuer = configuration["JwtSettings:Issuer"];
                var audience = configuration["JwtSettings:Audience"];
                var secret = configuration["JwtSettings:Secret"];

                if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(secret))
                {
                    throw new InvalidOperationException(@"Настройки JwtSettings (Issuer, Audience, Secret, LifetimeInMinutes)
                        обязательно должны присутствовать в конфигурационном файле.");
                }

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "unique_name",
                    RoleClaimType = "role",

                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        await WriteProblemDetailsAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "Unauthorized",
                            "Для выполнения этого действия необходимо войти в систему");
                    },
                    OnForbidden = async context =>
                    {
                        await WriteProblemDetailsAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "Forbidden",
                            "У вас недостаточно прав для выполнения этой операции.");
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddSwaggerForJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Description = "Введите только ваш JWT токен (слово Bearer добавлять не нужно)."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });

            return services;
        }
        
        public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;
            var serviceName = configuration["Otlp:ServiceName"] ?? "booking-service";
            var otlpEndpoint = configuration["Otlp:Endpoint"] ?? "http://localhost:4317";

            builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.WithProperty("service", serviceName)
                .WriteTo.Console(new CompactJsonFormatter()));

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName: serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(options =>
                        options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/metrics"))
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter());

            return builder;
        }

        private static Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = factory.CreateProblemDetails(
                context,
                statusCode: statusCode,
                title: title,
                detail: detail);

            return context.Response.WriteAsJsonAsync(problemDetails,
                options: null,
                contentType: "application/problem+json");
        }
    }
}
