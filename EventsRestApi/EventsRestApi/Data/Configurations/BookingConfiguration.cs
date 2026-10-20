using EventsRestApi.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsRestApi.Presentation.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder
                .ToTable(b =>
                {
                    b.HasCheckConstraint("CK_Booking_BookedSeats", "booked_seats > 0");
                    b.HasCheckConstraint("CK_Booking_Dates", "processed_at IS NULL OR created_at <= processed_at");
                });

            builder
                .Property(b => b.Id)
                .ValueGeneratedNever();

            builder
                .Property(b => b.Status)
                .HasConversion<string>();

            builder
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId);
        }
    }
}
