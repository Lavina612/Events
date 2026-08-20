using EventsRestApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Middlewares
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

        private ProblemDetails CreateProblemDetails(Exception ex)
        {
            var error = new ProblemDetails
            {
                Detail = ex.Message
            };

            switch (ex)
            {
                case AppValidationException ave:
                    error.Status = StatusCodes.Status400BadRequest;
                    error.Extensions["errors"] = ave.Errors;
                    break;

                case NotFoundEventException nfee:
                    error.Status = StatusCodes.Status404NotFound;
                    error.Extensions["eventId"] = nfee.EventId;
                    break;

                case NoAvailableSeatsException nase:
                    error.Status = StatusCodes.Status409Conflict;
                    error.Extensions["eventId"] = nase.EventId;
                    error.Extensions["requestedSeats"] = nase.RequestedSeats;
                    error.Extensions["availableSeats"] = nase.AvailableSeats;
                    break;

                case FinishedEventException fee:
                    error.Status = StatusCodes.Status422UnprocessableEntity;
                    error.Extensions["eventId"] = fee.EventId;
                    error.Extensions["endAt"] = fee.EndAt;
                    break;

                default:
                    error.Status = StatusCodes.Status500InternalServerError;
                    break;
            }

            return error;
        }
    }
}
