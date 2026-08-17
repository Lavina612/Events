namespace EventsRestApi.Models
{
    public class Event
    {
        public Guid Id { get; init; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public int TotalSeats { get; init; }

        public int AvailableSeats { get; set; }

        public Event(
            Guid id,
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
        }

        public bool TryReserveSeats(int count = 1)
        {
            if (AvailableSeats < count)
            {
                return false;
            }

            AvailableSeats -= count;
            return true;
        }

        public void ReleaseSeats(int count = 1)
        {
            AvailableSeats += count;

            if (AvailableSeats > TotalSeats)
            {
                throw new Exception("По непонятной причине кол-во доступных мест на мероприятие стало больше, чем общее кол-во мест.");
            }
        }
    }
}
