using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Models;

namespace EventsRestApi.Mappers
{
    public static class Mapper
    {
        public static Event MapToEvent(Guid eventId, EventRequestDto eventDto)
        {
            return new Event(
                eventId,
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

        public static BookingResponseDto MapToBookingResponseDto(Booking booking)
        {
            return new BookingResponseDto(
                booking.Id,
                booking.EventId,
                booking.Status,
                booking.CreatedAt,
                booking.ProcessedAt);
        }
    }
}
