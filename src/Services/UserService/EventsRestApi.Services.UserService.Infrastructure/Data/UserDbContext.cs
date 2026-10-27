using EventsRestApi.Services.UserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services.UserService.Infrastructure.Data
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
        }
    }
}
