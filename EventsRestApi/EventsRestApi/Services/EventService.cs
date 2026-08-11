using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        private readonly TimeProvider _timeProvider;

        public EventService(
            IEventRepository eventRepository,
            TimeProvider timeProvider)
        {
            _eventRepository = eventRepository;
            _timeProvider = timeProvider;
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
                filteredEventsForPageEnumerable.Select(Mapper.MapToEventResponseDto).ToList(),
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
                : Mapper.MapToEventResponseDto(foundEvent);
        }

        public EventResponseDto Add(EventRequestDto addingEventDto)
        {
            var addedEvent = _eventRepository.Add(Mapper.MapToEvent(addingEventDto));

            return Mapper.MapToEventResponseDto(addedEvent);
        }

        public bool Update(Guid id, EventRequestDto newEventDto)
        {
            var newEvent = Mapper.MapToEvent(newEventDto);
            newEvent.Id = id;

            return _eventRepository.Update(newEvent);
        }

        public bool Delete(Guid id)
        {
            return _eventRepository.Delete(id);
        }

        public bool IsEventStillValid(Guid id)
        {
            var foundEvent = _eventRepository.GetById(id);

            if (foundEvent == null)
            {
                return false;
            }

            return foundEvent.EndAt > _timeProvider.GetUtcNow().UtcDateTime;
        }
    }
}
