using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
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

        public async Task<int> GetBookingsCount(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Bookings.CountAsync(x => x.UserId == id, cancellationToken);
        }
    }
}
