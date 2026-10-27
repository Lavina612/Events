namespace EventsRestApi.Services.SharedContracts.MessageBroker
{
    public record BookingCancelledEvent(
        Guid BookingId,
        Guid EventId,
        Guid UserId,
        int BookedSeats,
        DateTime CancelledAt);
}
