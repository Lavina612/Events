namespace EventsRestApi.Exceptions
{
    public class NoAvailableSeatsException : Exception
    {
        public Guid EventId { get; }

        public int RequestedSeats { get; }

        public int AvailableSeats { get; }

        public NoAvailableSeatsException(Guid eventId, int requestedSeats, int availableSeats)
            : base($"На мероприятии с Id: {eventId} недостаточно мест. " +
                  $"Запрошено мест: {requestedSeats}, доступно мест: {availableSeats}.")
        {
            EventId = eventId;
            RequestedSeats = requestedSeats;
            AvailableSeats = availableSeats;
        }
    }
}
