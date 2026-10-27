using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Models;

namespace EventsRestApi.Services.EventService.Application.Services.Interfaces
{
    public interface IEventService
    {
        Task<PaginatedResult<Event>> GetPageAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken);
        Task<List<Event>> GetTop10PopularAsync(CancellationToken cancellationToken);
        Task<Event> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Event updatingEvent, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
