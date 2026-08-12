using EventsRestApi.Models;

namespace EventsRestApi.Dto.Response
{
    public record BookingResponseDto(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt = null);
}
