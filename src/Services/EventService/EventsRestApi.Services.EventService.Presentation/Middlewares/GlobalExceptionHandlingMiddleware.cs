using EventsRestApi.Services.EventService.Domain.Exceptions;
using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Middlewares;

namespace EventsRestApi.Services.EventService.Presentation.Middlewares
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

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            return (statusCode, extensions);
        }
    }
}
