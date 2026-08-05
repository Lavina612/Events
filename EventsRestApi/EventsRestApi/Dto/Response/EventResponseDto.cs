namespace EventsRestApi.Dto.Response
{
    public class EventResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public EventResponseDto(
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