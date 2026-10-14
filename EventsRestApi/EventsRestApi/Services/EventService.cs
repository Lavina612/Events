using EventsRestApi.Data.Repositories.Interfaces;
using EventsRestApi.Dto.Response;
using EventsRestApi.Models;
using EventsRestApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<PaginatedResult<Event>> GetPageAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _eventRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(x => x.Title.Contains(title, StringComparison.CurrentCultureIgnoreCase));
            }

            if (from != null)
            {
                query = query.Where(x => x.StartAt >= from);
            }

            if (to != null)
            {
                query = query.Where(x => x.EndAt <= to);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var filteredEventsForPage = await query
                .OrderBy(x => x.StartAt)
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Event>(
                filteredEventsForPage,
                totalCount,
                page,
                pageSize);
        }

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _eventRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken)
        {
            return await _eventRepository.AddAsync(addingEvent, cancellationToken);
        }

        public async Task<bool> UpdateAsync(Event updatingEvent, CancellationToken cancellationToken)
        {
            return await _eventRepository.UpdateAsync(updatingEvent, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _eventRepository.DeleteAsync(id, cancellationToken);
        }
    }
}
