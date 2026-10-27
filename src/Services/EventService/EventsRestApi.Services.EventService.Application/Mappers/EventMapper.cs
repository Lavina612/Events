using EventsRestApi.Services.EventService.Application.Dto.Request;
using EventsRestApi.Services.EventService.Application.Dto.Response;
using EventsRestApi.Services.EventService.Domain.Models.Entities;

namespace EventsRestApi.Services.EventService.Application.Mappers
{
    public static class EventMapper
    {
        public static Event MapToEvent(Guid eventId, EventRequestDto eventDto)
        {
            return new Event(
                eventId,
                eventDto.Title,
                eventDto.Description,
                eventDto.StartAt,
                eventDto.EndAt,
                eventDto.TotalSeats);
        }

        public static Event MapToEvent(EventRequestDto eventDto)
        {
            return MapToEvent(Guid.Empty, eventDto);
        }

        public static EventResponseDto MapToEventResponseDto(Event ev)
        {
            return new EventResponseDto(
                ev.Id,
                ev.Title,
                ev.Description,
                ev.StartAt,
                ev.EndAt,
                ev.TotalSeats,
                ev.AvailableSeats);
        }
    }
}
