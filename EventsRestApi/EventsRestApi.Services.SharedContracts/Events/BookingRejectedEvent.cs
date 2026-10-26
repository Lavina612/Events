namespace EventsRestApi.Services.SharedContracts.Events
{
    public record BookingRejectedEvent(
        Guid BookingId,
        Guid EventId,
        Guid UserId,
        int BookedSeats,
        string Reason,
        DateTime RejectedAt);
}
