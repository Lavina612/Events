namespace EventsRestApi.Domain.Models.Entities
{
    public class User
    {
        public Guid Id { get; }

        public string Login { get; private set; }

        public string HashedPassword { get; private set; }

        public UserRole Role { get; private set; }

        public List<Booking> Bookings { get; private set; } = new();

        private User() 
        {
            Login = null!;
            HashedPassword = null!;
        }

        public User(
            Guid id,
            string login,
            string hashPassword,
            UserRole role)
            : this()
        {
            Id = id;
            Login = login;
            HashedPassword = hashPassword;
            Role = role;
        }
    }
}
