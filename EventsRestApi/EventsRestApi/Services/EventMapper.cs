using EventsRestApi.Dto;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public static class EventMapper
    {
        public static Event MapToEvent(EventDto eventDto)
        {
            return new Event(
                eventDto.Id,
                eventDto.Title,
                eventDto.Description,
                eventDto.StartAt,
                eventDto.EndAt);
        }

        public static EventDto MapToEventDto(Event ev)
        {
            return new EventDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                StartAt = ev.StartAt,
                EndAt = ev.EndAt
            };
        }
    }
}
