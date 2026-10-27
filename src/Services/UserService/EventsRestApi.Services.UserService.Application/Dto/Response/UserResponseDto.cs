using EventsRestApi.Services.SharedContracts.Models;

namespace EventsRestApi.Services.UserService.Application.Dto.Response
{
    public record UserResponseDto(
        Guid Id,
        string Login,
        UserRole Role);
}
