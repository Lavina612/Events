namespace EventsRestApi.Domain.Models.Entities
{
    public class User
    {
        public Guid Id { get; }

        public string Login { get; set; }

        public string HashedPassword { get; set; }

        public UserRole Role { get; set; }

        public List<Booking> Bookings { get; private set; }

        public User() { }

        public User(
            Guid id,
            string login,
            string hashPassword,
            UserRole role)
        {
            Id = id;
            Login = login;
            HashedPassword = hashPassword;
            Role = role;
        }
    }
}
