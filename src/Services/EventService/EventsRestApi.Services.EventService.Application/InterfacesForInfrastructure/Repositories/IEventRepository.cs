using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Models;

namespace EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories
{
    public interface IEventRepository
    {
        Task<PaginatedResult<Event>> GetPageAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken);
        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Event newEvent, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task SaveContextAsync(CancellationToken cancellationToken);
    }
}
