using EventsRestApi.Application.Dto.Request;
using EventsRestApi.Application.Dto.Response;
using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Mappers
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

        public static BookingResponseDto MapToBookingResponseDto(Booking booking)
        {
            return new BookingResponseDto(
                booking.Id,
                booking.EventId,
                booking.UserId,
                booking.Status,
                booking.CreatedAt,
                booking.ProcessedAt);
        }

        public static UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto(
                user.Id,
                user.Login,
                user.Role);
        }
    }
}
