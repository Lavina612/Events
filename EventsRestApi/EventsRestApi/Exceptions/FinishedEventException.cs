namespace EventsRestApi.Exceptions
{
    public class FinishedEventException : Exception
    {
        public Guid EventId { get; }

        public DateTime EndAt { get; }

        public FinishedEventException(Guid eventId, DateTime endAt)
            : base($"Событие с Id = {eventId} уже завершилось. Дата завершениея: {endAt}.")
        {
            EventId = eventId;
            EndAt = endAt;
        }
    }
}
