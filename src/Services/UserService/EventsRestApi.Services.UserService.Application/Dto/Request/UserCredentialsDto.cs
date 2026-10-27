using System.ComponentModel.DataAnnotations;

namespace EventsRestApi.Services.UserService.Application.Dto.Request
{
    public record UserCredentialsDto(
        [Required(ErrorMessage = "Логин обязателен для заполнения.")] string Login,
        [Required(ErrorMessage = "Пароль обязателен для заполнения.")] string Password);
}
