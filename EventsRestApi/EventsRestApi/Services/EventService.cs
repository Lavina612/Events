using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public PaginatedResult<Event> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var filteredEventsEnumerable = _eventRepository.Get().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                filteredEventsEnumerable = filteredEventsEnumerable.Where(x => x.Title.Contains(title, StringComparison.CurrentCultureIgnoreCase));
            }

            if (from != null)
            {
                filteredEventsEnumerable = filteredEventsEnumerable.Where(x => x.StartAt >= from);
            }

            if (to != null)
            {
                filteredEventsEnumerable = filteredEventsEnumerable.Where(x => x.EndAt <= to);
            }

            var filteredEvents = filteredEventsEnumerable.ToList();

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

        public Event? GetById(Guid id)
        {
            return _eventRepository.GetById(id);
        }

        public Event Add(Event addingEvent)
        {
            return _eventRepository.Add(addingEvent);
        }

        public bool Update(Event updatingEvent)
        {
            return _eventRepository.Update(updatingEvent);
        }

        public bool Delete(Guid id)
        {
            return _eventRepository.Delete(id);
        }
    }
}
