using EventsRestApi.Application.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace EventsRestApi.Application.Services
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(
                    $"Пароль не может быть пустым.",
                    nameof(password));
            }

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var verifyingHashedPassword = HashPassword(password);

            return string.Equals(verifyingHashedPassword, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
