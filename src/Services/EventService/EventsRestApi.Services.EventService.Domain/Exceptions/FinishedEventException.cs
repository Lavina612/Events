namespace EventsRestApi.Services.EventService.Domain.Exceptions
{
    public class FinishedEventException : Exception
    {
        public Guid EventId { get; }

        public DateTime EventEndAt { get; }

        public FinishedEventException(Guid eventId, DateTime endAt)
            : base($"Событие с Id = {eventId} уже завершилось. Дата завершения: {endAt}.")
        {
            EventId = eventId;
            EventEndAt = endAt;
        }
    }
}
