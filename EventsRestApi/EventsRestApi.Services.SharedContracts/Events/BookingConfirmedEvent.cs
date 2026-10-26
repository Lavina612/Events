namespace EventsRestApi.Services.SharedContracts.Events
{
    public record BookingConfirmedEvent(
        Guid BookingId,
        Guid EventId,
        Guid UserId,
        int BookedSeats,
        DateTime ConfirmedAt);
}
