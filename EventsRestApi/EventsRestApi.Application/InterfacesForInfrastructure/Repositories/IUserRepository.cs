using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.InterfacesForInfrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);
        Task<User> AddAsync(string login, string hashPassword, CancellationToken cancellationToken);
        Task<int> GetBookingsCountAsync(Guid id, CancellationToken cancellationToken);
    }
}
