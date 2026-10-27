namespace EventsRestApi.Services.UserService.Application.Services.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        void VerifyPassword(string password, string hashedPassword);
    }
}
