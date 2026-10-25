namespace EventsRestApi.Domain.Models.Entities
{
    public class Booking
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public Event Event { get; private set; }

        public Guid UserId { get; private set; }

        public User User { get; private set; }

        public BookingStatus Status { get; private set; }

        public int BookedSeats { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        private Booking() 
        { 
            Event = null!; 
            User = null!; 
        }

        public Booking(
            Guid id,
            Guid eventId,
            Guid userId,
            BookingStatus status,
            int bookedSeats,
            DateTime createdAt)
            : this()
        {
            Id = id;

            if (bookedSeats <= 0)
            {
                throw new ArgumentException(
                    $"Бронирование с Id: {Id}: Количество бронируемых мест должно быть положительным.",
                    nameof(BookedSeats));
            }

            UserId = userId;
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

        public void Cancel(DateTime currentUtcNow)
        {
            ChangeStatus(BookingStatus.Cancelled, currentUtcNow);
        }

        public void ChangeStatus(BookingStatus status, DateTime? currentUtcNow)
        {
            if (currentUtcNow.HasValue && currentUtcNow < CreatedAt)
            {
                throw new ArgumentException(
                    $"Бронирование с Id: {Id}: Дата обработки бронирования не может быть меньше даты создания.",
                    nameof(ProcessedAt));
            }

            Status = status;
            ProcessedAt = currentUtcNow;
        }
    }
}
