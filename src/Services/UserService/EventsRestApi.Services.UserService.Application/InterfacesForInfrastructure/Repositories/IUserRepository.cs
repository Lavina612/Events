using EventsRestApi.Services.SharedContracts.Models;
using EventsRestApi.Services.UserService.Domain.Models.Entities;

namespace EventsRestApi.Services.UserService.Application.InterfacesForInfrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);
        Task<User> AddAsync(string login, string hashedPassword, UserRole role, CancellationToken cancellationToken);
    }
}
