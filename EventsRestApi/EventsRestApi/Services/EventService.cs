using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
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

        public PaginatedResult<EventResponseDto> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize)
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

            return new PaginatedResult<EventResponseDto>(
                filteredEventsForPageEnumerable.Select(EventMapper.MapToEventResponseDto).ToList(),
                totalFilteredEventsCount,
                page,
                pageSize,
                totalPages);
        }

        public EventResponseDto? GetById(Guid id)
        {
            var foundEvent = _eventRepository.GetById(id);

            return foundEvent == null
                ? null
                : EventMapper.MapToEventResponseDto(foundEvent);
        }

        public EventResponseDto Add(EventRequestDto addingEventDto)
        {
            var addedEvent = _eventRepository.Add(EventMapper.MapToEvent(addingEventDto));

            return EventMapper.MapToEventResponseDto(addedEvent);
        }

        public bool Update(Guid id, EventRequestDto newEventDto)
        {
            return _eventRepository.Update(id, EventMapper.MapToEvent(newEventDto));
        }

        public bool Delete(Guid id)
        {
            return _eventRepository.Delete(id);
        }
    }
}
