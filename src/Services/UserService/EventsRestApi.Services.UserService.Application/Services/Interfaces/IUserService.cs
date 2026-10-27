using EventsRestApi.Services.SharedContracts.Models;
using EventsRestApi.Services.UserService.Application.Dto.Request;
using EventsRestApi.Services.UserService.Domain.Models.Entities;

namespace EventsRestApi.Services.UserService.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<User> RegisterAsync(UserCredentialsDto userCredentialsDto, UserRole role, CancellationToken cancellationToken);
        Task<string> LogInAsync(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken);
    }
}
