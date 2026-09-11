using EventsRestApi.Data.Repositories.Interfaces;
using EventsRestApi.Dto.Response;
using EventsRestApi.Models;
using EventsRestApi.Services.Interfaces;

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
            var allEvents = await _eventRepository.GetAllAsync(cancellationToken);
            var filteringEventsEnumerable = allEvents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                filteringEventsEnumerable = filteringEventsEnumerable.Where(x => x.Title.Contains(title, StringComparison.CurrentCultureIgnoreCase));
            }

            if (from != null)
            {
                filteringEventsEnumerable = filteringEventsEnumerable.Where(x => x.StartAt >= from);
            }

            if (to != null)
            {
                filteringEventsEnumerable = filteringEventsEnumerable.Where(x => x.EndAt <= to);
            }

            var filteredEvents = filteringEventsEnumerable.ToList();

            var totalFilteredEventsCount = filteredEvents.Count;
            var totalPages = (totalFilteredEventsCount + pageSize - 1) / pageSize;

            var filteredEventsForPage = filteredEvents
                .OrderBy(x => x.StartAt)
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToList();

            return new PaginatedResult<Event>(
                filteredEventsForPage,
                totalFilteredEventsCount,
                page,
                pageSize,
                totalPages);
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
