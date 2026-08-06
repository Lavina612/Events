namespace EventsRestApi.Models
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public Booking(
            Guid id,
            Guid eventId,
            BookingStatus status,
            DateTime createdAt,
            DateTime? processedAt = null)
        {
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
        }
    }
}
