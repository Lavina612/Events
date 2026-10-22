namespace EventsRestApi.Application.InterfacesForInfrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<int> GetBookingsCount(Guid id, CancellationToken cancellationToken);
    }
}
