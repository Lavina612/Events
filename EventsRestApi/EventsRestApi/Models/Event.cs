namespace EventsRestApi.Models
{
    public class Event
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public Event() { }

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
