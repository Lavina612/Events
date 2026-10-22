namespace EventsRestApi.Domain.Exceptions
{
    public class StartedEventException : Exception
    {
        public Guid EventId { get; set; }

        public DateTime EventStartAt { get; set; }

        public StartedEventException(Guid eventId, DateTime eventStartAt)
            : base($"Невозможно создать бронирование на событие с Id = {eventId}, т.к. оно уже началось (дата начала: {eventStartAt}).")
        {
            EventId = eventId;
            EventStartAt = eventStartAt;
        }
    }
}
