using EventsRestApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsRestApi.Data.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Event_Dates", "start_at < end_at");
                    t.HasCheckConstraint("CK_Event_TotalSeats", "total_seats > 0");
                    t.HasCheckConstraint("CK_Event_AvailableSeats", "available_seats >= 0 AND available_seats <= total_seats");
                });


            builder
                .Property(e => e.Id)
                .ValueGeneratedNever();

            builder
                .Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder
                .Property(e => e.Description)
                .HasMaxLength(2000);

            builder
                .Property(e => e.StartAt)
                .HasField("_startAt");

            builder
                .Property(e => e.EndAt)
                .HasField("_endAt");

            builder
                .Property(e => e.TotalSeats)
                .HasField("_totalSeats");

            builder
                .Property(e => e.AvailableSeats)
                .HasField("_availableSeats");

            builder
                .HasMany(e => e.Bookings)
                .WithOne(b => b.Event)
                .HasForeignKey(b => b.EventId);
        }
    }
}
