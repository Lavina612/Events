using EventsRestApi.Presentation.Models.Entities;

namespace EventsRestApi.Presentation.Dto.Response
{
    public record BookingResponseDto(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt = null);
}
