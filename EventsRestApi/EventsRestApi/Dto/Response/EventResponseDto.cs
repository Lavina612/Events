namespace EventsRestApi.Dto.Response
{
    public record EventResponseDto(
        Guid Id,
        string Title,
        string? Description,
        DateTime StartAt,
        DateTime EndAt);
}