using EventsRestApi.Models;

namespace EventsRestApi.Data.Repositories.Interfaces
{
    public interface IEventRepository
    {
        Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken);
        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Event newEvent, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task SaveContextAsync(CancellationToken cancellationToken);
    }
}
