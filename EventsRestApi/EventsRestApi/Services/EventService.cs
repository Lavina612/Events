using EventsRestApi.Dto;
using EventsRestApi.Interfaces;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public PaginatedResult<EventDto> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize)
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

            var filteredEventsForPageEnumerable = filteredEvents
                .OrderBy(x => x.StartAt)
                .Skip(pageSize * (page - 1))
                .Take(pageSize);

            return new PaginatedResult<EventDto>(
                filteredEventsForPageEnumerable.Select(EventMapper.MapToEventDto).ToList(),
                totalFilteredEventsCount,
                page,
                pageSize,
                totalPages);
        }

        public EventDto? GetById(Guid id)
        {
            var foundEvent = _eventRepository.GetById(id);

            return foundEvent == null
                ? null
                : EventMapper.MapToEventDto(foundEvent);
        }

        public EventDto Add(EventDto addingEventDto)
        {
            var addedEvent = _eventRepository.Add(EventMapper.MapToEvent(addingEventDto));

            return EventMapper.MapToEventDto(addedEvent);
        }

        public bool Update(Guid id, EventDto newEventDto)
        {
            return _eventRepository.Update(id, EventMapper.MapToEvent(newEventDto));
        }

        public bool Delete(Guid id)
        {
            return _eventRepository.Delete(id);
        }
    }
}
