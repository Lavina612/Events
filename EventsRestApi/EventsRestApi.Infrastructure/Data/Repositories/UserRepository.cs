using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(x =>  x.Login == login, cancellationToken);
        }

        public async Task<User> AddAsync(string login, string hashPassword, CancellationToken cancellationToken)
        {
            var user = new User(
                Guid.NewGuid(),
                login,
                hashPassword,
                UserRole.User);

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return user;
        }

        public async Task<int> GetBookingsCountAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Bookings.CountAsync(x => x.UserId == id, cancellationToken);
        }
    }
}
