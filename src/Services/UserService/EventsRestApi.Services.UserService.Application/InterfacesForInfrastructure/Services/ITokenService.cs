using EventsRestApi.Services.SharedContracts.Models;

namespace EventsRestApi.Services.UserService.Application.InterfacesForInfrastructure.Services
{
    public interface ITokenService
    {
        string GenerateToken(Guid id, string login, UserRole role);
    }
}
