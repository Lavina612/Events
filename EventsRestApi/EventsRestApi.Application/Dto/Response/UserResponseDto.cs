using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Dto.Response
{
    public record UserResponseDto(
        Guid Id,
        string Login,
        UserRole Role);
}
