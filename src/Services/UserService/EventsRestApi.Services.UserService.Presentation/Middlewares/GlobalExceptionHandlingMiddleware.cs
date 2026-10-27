using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Middlewares;
using EventsRestApi.Services.UserService.Application.Exceptions;

namespace EventsRestApi.Services.UserService.Presentation.Middlewares
{
    public class GlobalExceptionHandlingMiddleware : BaseGlobalExceptionHandlingMiddleware
    {
        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
            : base(next, logger) { }

        protected override (int statusCode, Dictionary<string, object?> extensions) MapExceptionToDetails(Exception exception)
        {
            var extensions = new Dictionary<string, object?>();
            int statusCode;

            switch (exception)
            {
                case ArgumentException ex:
                    statusCode = StatusCodes.Status400BadRequest;
                    extensions["paramName"] = ex.ParamName;
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
