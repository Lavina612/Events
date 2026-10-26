namespace EventsRestApi.Services.SharedContracts.Events
{
    public record BookingRejectedEvent(
        Guid BookingId,
        Guid EventId,
        Guid UserId,
        string Reason,
        DateTime RejectedAt);
}
