using EventsRestApi.Services.EventService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services.EventService.Infrastructure.Data
{
    public class EventDbContext : DbContext
    {
        public DbSet<Event> Events => Set<Event>();

        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
        }
    }
}
