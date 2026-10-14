using EventsRestApi.Models;

namespace EventsRestApi.Data.Repositories.Interfaces
{
    public interface IEventRepository
    {
        IQueryable<Event> GetAllAsync();
        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Event newEvent, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task SaveContextAsync(CancellationToken cancellationToken);
    }
}
