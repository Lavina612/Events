using EventsRestApi.Services.UserService.Application.Extensions;
using EventsRestApi.Services.UserService.Infrastructure.Extensions;
using EventsRestApi.Services.UserService.Presentation.Extensions;
using EventsRestApi.Services.UserService.Presentation.Middlewares;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSwaggerForJwt();

builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
