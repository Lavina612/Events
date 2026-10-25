using EventsRestApi.Application.Dto.Request;
using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task EnsureCanMakeBookingAsync(Guid userId, CancellationToken cancellationToken);
        Task<User> RegisterAsync(UserCredentialsDto userCredentialsDto, UserRole role, CancellationToken cancellationToken);
        Task<string> LogInAsync(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken);
    }
}
