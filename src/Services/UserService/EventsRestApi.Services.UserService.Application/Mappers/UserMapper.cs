using EventsRestApi.Services.UserService.Application.Dto.Response;
using EventsRestApi.Services.UserService.Domain.Models.Entities;

namespace EventsRestApi.Services.UserService.Application.Mappers
{
    public static class UserMapper
    {
        public static UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto(
                user.Id,
                user.Login,
                user.Role);
        }
    }
}
