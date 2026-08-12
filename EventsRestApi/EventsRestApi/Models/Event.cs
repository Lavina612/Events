namespace EventsRestApi.Models
{
    public class Event
    {
        public Guid Id { get; init; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public Event(
            Guid id,
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
