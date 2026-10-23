using EventsRestApi.Application.Exceptions;
using EventsRestApi.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Presentation.Middlewares
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex);
            }
        }

        private async Task HandleException(HttpContext httpContext, Exception ex)
        {
            var error = CreateProblemDetails(ex);

            if (error.Status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception. Method = {Method}, Path = {Path}, RequestId = {RequestId}.",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    httpContext.Request.Headers["x-request-id"]);
            }
            else
            {
                _logger.LogWarning(
                    "Business exception ({StatusCode}): {Message}. Method = {Method}, Path = {Path}, RequestId = {RequestId}.",
                    error.Status,
                    ex.Message.TrimEnd('.'),
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    httpContext.Request.Headers["x-request-id"]);
            }

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            httpContext.Response.StatusCode = error.Status ?? StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(error);
        }

        private ProblemDetails CreateProblemDetails(Exception exception)
        {
            var error = new ProblemDetails
            {
                Detail = exception.Message
            };

            switch (exception)
            {
                case ArgumentException ex:
                    error.Status = StatusCodes.Status400BadRequest;
                    error.Extensions["paramName"] = ex.ParamName;
                    break;

                case FinishedEventException ex:
                    error.Status = StatusCodes.Status400BadRequest;
                    error.Extensions["eventId"] = ex.EventId;
                    error.Extensions["endAt"] = ex.EventEndAt;
                    break;

                case StartedEventException ex:
                    error.Status = StatusCodes.Status400BadRequest;
                    error.Extensions["eventId"] = ex.EventId;
                    error.Extensions["startAt"] = ex.EventStartAt;
                    break;

                case UnauthorizedAccessException ex:
                    error.Status = StatusCodes.Status401Unauthorized;
                    break;

                case ForbiddenException ex:
                    error.Status = StatusCodes.Status403Forbidden;
                    error.Extensions["userId"] = ex.UserId;
                    break;

                case BaseNotFoundException ex:
                    error.Status = StatusCodes.Status404NotFound;
                    error.Extensions["Id"] = ex.Id;
                    error.Extensions["resourceType"] = ex.ResourceName;
                    break;

                case NoAvailableSeatsException ex:
                    error.Status = StatusCodes.Status409Conflict;
                    error.Extensions["eventId"] = ex.EventId;
                    error.Extensions["requestedSeats"] = ex.RequestedSeats;
                    error.Extensions["availableSeats"] = ex.AvailableSeats;
                    break;

                case ExceedingActiveBookingLimitException ex:
                    error.Status = StatusCodes.Status409Conflict;
                    error.Extensions["userId"] = ex.UserId;
                    error.Extensions["bookingLimit"] = ex.BookingLimit;
                    break;

                case UserAlreadyExistsException ex:
                    error.Status = StatusCodes.Status409Conflict;
                    error.Extensions["login"] = ex.Login;
                    break;

                default:
                    error.Status = StatusCodes.Status500InternalServerError;
                    break;
            }

            return error;
        }
    }
}
