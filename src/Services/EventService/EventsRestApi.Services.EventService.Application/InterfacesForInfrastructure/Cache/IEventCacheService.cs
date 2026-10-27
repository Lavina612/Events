namespace EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Cache
{
    public interface IEventCacheService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value, TimeSpan expiration);
        Task DeleteAsync(string key);
    }
}
