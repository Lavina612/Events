using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace EventsRestApi.Services.SharedContracts.Middlewares
{
    public abstract class BaseGlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaseGlobalExceptionHandlingMiddleware> _logger;

        public BaseGlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<BaseGlobalExceptionHandlingMiddleware> logger)
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
                detail: statusCode >= StatusCodes.Status500InternalServerError
                    ? "Неожиданная ошибка на стороне сервера. Попробуйте повторить попытку позже."
                    : ex.Message);

            foreach (var ext in extensions)
            {
                problemDetails.Extensions[ext.Key] = ext.Value;
            }

            if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
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

        protected abstract (int statusCode, Dictionary<string, object?> extensions) MapExceptionToDetails(Exception exception);
    }
}
