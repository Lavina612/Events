using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public static class EventMapper
    {
        public static Event MapToEvent(EventRequestDto eventDto)
        {
            return new Event(
                Guid.Empty,
                eventDto.Title,
                eventDto.Description,
                eventDto.StartAt,
                eventDto.EndAt);
        }

        public static EventResponseDto MapToEventResponseDto(Event ev)
        {
            return new EventResponseDto(
                ev.Id,
                ev.Title,
                ev.Description,
                ev.StartAt,
                ev.EndAt);
        }
    }
}
