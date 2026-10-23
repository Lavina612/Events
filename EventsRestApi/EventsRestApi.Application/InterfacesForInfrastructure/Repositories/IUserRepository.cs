using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.InterfacesForInfrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);
        Task<User> AddAsync(string login, string hashedPassword, UserRole role, CancellationToken cancellationToken);
        Task<int> GetBookingsCountAsync(Guid id, CancellationToken cancellationToken);
    }
}
