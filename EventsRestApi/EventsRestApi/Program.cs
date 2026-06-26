using EventsRestApi.Interfaces;
using EventsRestApi.Middlewares;
using EventsRestApi.Models;
using EventsRestApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var initialEvents = new List<Event>();

builder.Services.AddSingleton(initialEvents);
builder.Services.AddScoped<IEventService, EventService>();
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
