namespace EventsRestApi.Services.SharedContracts.MessageBroker
{
    public record BookingConfirmedEvent(
        Guid BookingId,
        Guid EventId,
        Guid UserId,
        int BookedSeats,
        DateTime ConfirmedAt);
}
