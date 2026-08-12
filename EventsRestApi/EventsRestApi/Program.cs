using EventsRestApi.Interfaces;
using EventsRestApi.Middlewares;
using EventsRestApi.Repositories;
using EventsRestApi.Services;
using EventsRestApi.Settings;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<BookingBackgroundServiceSettings>(
    builder.Configuration.GetSection(BookingBackgroundServiceSettings.SectionName));

builder.Services.AddHostedService<BookingBackgroundService>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
