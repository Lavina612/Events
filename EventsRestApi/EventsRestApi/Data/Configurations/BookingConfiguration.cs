using EventsRestApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsRestApi.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder
                .ToTable("bookings", b =>
                {
                    b.HasCheckConstraint("CK_Booking_BookedSeats", "\"BookedSeats\" > 0");
                    b.HasCheckConstraint("CK_Booking_Dates", "\"ProcessedAt\" IS NULL OR \"CreatedAt\" <= \"ProcessedAt\"");
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
