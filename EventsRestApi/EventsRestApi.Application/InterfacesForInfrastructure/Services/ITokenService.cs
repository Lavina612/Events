using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.InterfacesForInfrastructure.Services
{
    public interface ITokenService
    {
        string GenerateToken(Guid id, string login, UserRole role);
    }
}
