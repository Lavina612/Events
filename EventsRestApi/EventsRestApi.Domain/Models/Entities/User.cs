namespace EventsRestApi.Domain.Models.Entities
{
    public class User
    {
        public Guid Id { get; }

        public string Login { get; set; }

        public string HashPassword { get; set; }

        public UserRole Role { get; set; }

        public User() { }

        public User(
            Guid id, 
            string login, 
            string hashPassword,
            UserRole role)
        {
            Id = id;
            Login = login;
            HashPassword = hashPassword;
            Role = role;
        }
    }
}
