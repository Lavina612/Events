using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Dto.Response
{
    public record BookingResponseDto(
        Guid Id,
        Guid EventId,
        Guid UserId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt = null);
}
