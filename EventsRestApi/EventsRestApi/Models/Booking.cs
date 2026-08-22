namespace EventsRestApi.Models
{
    public class Booking
    {
        public Guid Id { get; init; }

        public Guid EventId { get; init; }

        public BookingStatus Status { get; private set; }

        public int BookedSeats { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? ProcessedAt { get; private set; }

        public Booking(
            Guid id,
            Guid eventId,
            BookingStatus status,
            int bookedSeats,
            DateTime createdAt)
        {
            Id = id;

            if (bookedSeats <= 0)
            {
                throw new ArgumentException(
                    $"Бронирование с Id: {Id}: Количество бронируемых мест должно быть положительным.",
                    nameof(BookedSeats));
            }

            EventId = eventId;
            Status = status;
            BookedSeats = bookedSeats;
            CreatedAt = createdAt;
        }

        public void Confirm(DateTime currentUtcNow)
        {
            ChangeStatus(BookingStatus.Confirmed, currentUtcNow);
        }

        public void Reject(DateTime currentUtcNow)
        {
            ChangeStatus(BookingStatus.Rejected, currentUtcNow);
        }

        public void ChangeStatus(BookingStatus status, DateTime? currentUtcNow)
        {
            Status = status;
            ProcessedAt = currentUtcNow;
        }
    }
}
