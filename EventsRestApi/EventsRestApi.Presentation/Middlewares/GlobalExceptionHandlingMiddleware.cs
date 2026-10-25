using EventsRestApi.Application.Exceptions;
using EventsRestApi.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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

        public async Task InvokeAsync(HttpContext httpContext, ProblemDetailsFactory problemDetailsFactory)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex, problemDetailsFactory);
            }
        }

        private async Task HandleException(HttpContext httpContext, Exception ex, ProblemDetailsFactory problemDetailsFactory)
        {
            var (statusCode, extensions) = MapExceptionToDetails(ex);

            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: statusCode,
                detail: ex.Message);

            foreach (var ext in extensions)
            {
                problemDetails.Extensions[ext.Key] = ext.Value;
            }

            if (problemDetails.Status == StatusCodes.Status500InternalServerError)
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
                    problemDetails.Status,
                    ex.Message.TrimEnd('.'),
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    httpContext.Request.Headers["x-request-id"]);
            }

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails,
                options: null,
                contentType: "application/problem+json");
        }

        private static (int statusCode, Dictionary<string, object?> extensions) MapExceptionToDetails(Exception exception)
        {
            var extensions = new Dictionary<string, object?>();
            int statusCode;

            switch (exception)
            {
                case ArgumentException ex:
                    statusCode = StatusCodes.Status400BadRequest;
                    extensions["paramName"] = ex.ParamName;
                    break;

                case FinishedEventException ex:
                    statusCode = StatusCodes.Status400BadRequest;
                    extensions["eventId"] = ex.EventId;
                    extensions["eventEndAt"] = ex.EventEndAt;
                    break;

                case StartedEventException ex:
                    statusCode = StatusCodes.Status400BadRequest;
                    extensions["eventId"] = ex.EventId;
                    extensions["eventStartAt"] = ex.EventStartAt;
                    break;

                case InvalidCredentialsException ex:
                    statusCode = StatusCodes.Status401Unauthorized;
                    break;

                case InvalidTokenException ex:
                    statusCode = StatusCodes.Status401Unauthorized;
                    extensions["claimName"] = ex.ClaimName;
                    break;

                case ForbiddenException ex:
                    statusCode = StatusCodes.Status403Forbidden;
                    break;

                case BaseNotFoundException ex:
                    statusCode = StatusCodes.Status404NotFound;
                    extensions["resourceId"] = ex.ResourceId;
                    extensions["resourceType"] = ex.ResourceName;
                    break;

                case NoAvailableSeatsException ex:
                    statusCode = StatusCodes.Status409Conflict;
                    extensions["eventId"] = ex.EventId;
                    extensions["requestedSeats"] = ex.RequestedSeats;
                    extensions["availableSeats"] = ex.AvailableSeats;
                    break;

                case ExceedingActiveBookingLimitException ex:
                    statusCode = StatusCodes.Status409Conflict;
                    extensions["userId"] = ex.UserId;
                    extensions["bookingLimit"] = ex.BookingLimit;
                    break;

                case UserAlreadyExistsException ex:
                    statusCode = StatusCodes.Status409Conflict;
                    extensions["login"] = ex.Login;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            return (statusCode, extensions);
        }
    }
}
