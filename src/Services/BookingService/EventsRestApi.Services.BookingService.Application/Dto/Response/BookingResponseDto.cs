using EventsRestApi.Services.BookingService.Domain.Models.Entities;

namespace EventsRestApi.Services.BookingService.Application.Dto.Response
{
    public record BookingResponseDto(
        Guid Id,
        Guid EventId,
        Guid UserId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt = null);
}
