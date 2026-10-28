namespace EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Cache
{
    public interface ICacheService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value, TimeSpan expiration);
        Task DeleteAsync(string key);
    }
}
