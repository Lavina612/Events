using EventsRestApi.Services.BookingService.Application.Dto.Response;
using EventsRestApi.Services.BookingService.Domain.Models.Entities;

namespace EventsRestApi.Services.BookingService.Application.Mappers
{
    public static class BookingMapper
    {
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
    }
}
