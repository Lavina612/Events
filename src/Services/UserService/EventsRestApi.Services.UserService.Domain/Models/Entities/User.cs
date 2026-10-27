using EventsRestApi.Services.SharedContracts.Models;

namespace EventsRestApi.Services.UserService.Domain.Models.Entities
{
    public class User
    {
        public Guid Id { get; }

        public string Login { get; private set; }

        public string HashedPassword { get; private set; }

        public UserRole Role { get; private set; }

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
