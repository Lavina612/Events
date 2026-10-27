using EventsRestApi.Services.UserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsRestApi.Services.UserService.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .Property(u => u.Id)
                .ValueGeneratedNever();

            builder
                .Property(u => u.Role)
                .HasConversion<string>();

            builder
                .HasIndex(u => u.Login)
                .IsUnique();
        }
    }
}
