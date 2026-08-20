namespace EventsRestApi.Models
{
    public class Booking
    {
        public Guid Id { get; init; }

        public Guid EventId { get; init; }

        public BookingStatus Status { get; set; }

        public int BookedSeats { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? ProcessedAt { get; set; }

        public Booking(
            Guid id,
            Guid eventId,
            BookingStatus status,
            int bookedSeats,
            DateTime createdAt,
            DateTime? processedAt = null)
        {
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
            BookedSeats = bookedSeats;
        }
    }
}
