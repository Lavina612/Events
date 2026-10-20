using EventsRestApi.Application.RepositoryInterfaces;
using EventsRestApi.Application.Services;
using EventsRestApi.Application.Services.Interfaces;
using EventsRestApi.Application.Settings;
using EventsRestApi.Infrastructure.Data;
using EventsRestApi.Infrastructure.Data.Repositories;
using EventsRestApi.Presentation.Middlewares;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<BookingBackgroundServiceSettings>(
    builder.Configuration.GetSection(BookingBackgroundServiceSettings.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddHostedService<BookingBackgroundService>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
