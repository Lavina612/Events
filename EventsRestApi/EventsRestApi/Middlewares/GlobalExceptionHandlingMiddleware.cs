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
            _logger.LogError(
                ex,
                "Unhandled exception. Method = {Method}, Path = {Path}, RequestId = {RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.Headers["x-request-id"]);

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            var statusCode = MapStatusCode(ex);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var error = new ProblemDetails
            {
                Status = statusCode,
                Detail = ex.Message
            };

            if (ex is AppValidationException ave)
            {
                error.Extensions["errors"] = ave.Errors;
            }

            if (ex is NotFoundEventException nfee)
            {
                error.Extensions["eventId"] = nfee.EventId;
            }

            if (ex is NoAvailableSeatsException nase)
            {
                error.Extensions["eventId"] = nase.EventId;
                error.Extensions["requestedSeats"] = nase.RequestedSeats;
                error.Extensions["availableSeats"] = nase.AvailableSeats;
            }

            if (ex is FinishedEventException fee)
            {
                error.Extensions["eventId"] = fee.EventId;
                error.Extensions["endAt"] = fee.EndAt;
            }

            await httpContext.Response.WriteAsJsonAsync(error);
        }

        private static int MapStatusCode(Exception ex)
        {
            return ex switch
            {
                AppValidationException => StatusCodes.Status400BadRequest,
                NotFoundEventException => StatusCodes.Status404NotFound,
                NoAvailableSeatsException => StatusCodes.Status409Conflict,
                FinishedEventException => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}
