using EventsRestApi.Application.Dto.Request;
using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> CanMakeBooking(Guid userId, CancellationToken cancellationToken);
        Task<User> Register(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken);
        Task<string> LogIn(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken);
    }
}
