using EventsRestApi.Services.SharedContracts.Models;
using EventsRestApi.Services.UserService.Application.Exceptions;
using EventsRestApi.Services.UserService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.UserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventsRestApi.Services.UserService.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Login == login, cancellationToken);
        }

        public async Task<User> AddAsync(string login, string hashedPassword, UserRole role, CancellationToken cancellationToken)
        {
            var user = new User(
                Guid.NewGuid(),
                login,
                hashedPassword,
                role);

            try
            {
                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return user;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505") // ошибка нарушения unique
            {
                throw new UserAlreadyExistsException(login, ex);
            }
        }
    }
}
