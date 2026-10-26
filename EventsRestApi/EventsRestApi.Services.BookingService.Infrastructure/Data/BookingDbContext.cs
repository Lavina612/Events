using EventsRestApi.Services.BookingService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services.BookingService.Infrastructure.Data
{
    public class BookingDbContext : DbContext
    {
        public DbSet<Booking> Bookings => Set<Booking>();

        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
        }
    }
}
