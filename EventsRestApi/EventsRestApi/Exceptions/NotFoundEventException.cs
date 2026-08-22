namespace EventsRestApi.Exceptions
{
    public class NotFoundEventException : Exception
    {
        public Guid EventId { get; }

        public NotFoundEventException(Guid eventId)
            : base($"Событие с Id = {eventId} не найдено.")
        {
            EventId = eventId;
        }
    }
}
