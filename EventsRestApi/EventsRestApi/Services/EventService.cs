using EventsRestApi.Dto;
using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events;

        public EventService(List<Event> events)
        {
            _events = events;
        }

        public PaginatedResult<EventDto> GetAll(string? title, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var filteredEventsEnumerable = _events.AsEnumerable();

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
            var totalPages = (int) Math.Ceiling((double)totalFilteredEventsCount / pageSize);

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

        public EventDto? GetById(int id)
        {
            var foundEvent = _events.FirstOrDefault(x => x.Id == id);

            return foundEvent == null
                ? null
                : EventMapper.MapToEventDto(foundEvent);
        }

        public EventDto Add(EventDto addingEventDto)
        {
            addingEventDto.Id = _events.Any() ? _events.Max(x => x.Id) + 1 : 1;

            var addingEvent = EventMapper.MapToEvent(addingEventDto);

            _events.Add(addingEvent);

            return EventMapper.MapToEventDto(addingEvent);
        }

        public bool Update(int id, EventDto newEventDto)
        {
            var updatingEvent = _events.FirstOrDefault(x => x.Id == id);

            if (updatingEvent == null)
            {
                return false;
            }

            updatingEvent.Title = newEventDto.Title;
            updatingEvent.Description = newEventDto.Description;
            updatingEvent.StartAt = newEventDto.StartAt;
            updatingEvent.EndAt = newEventDto.EndAt;

            return true;
        }

        public bool Delete(int id)
        {
            var deletingEvent = _events.FirstOrDefault(x => x.Id == id);

            if (deletingEvent == null)
            {
                return false;
            }

            _events.Remove(deletingEvent);

            return true;
        }
    }
}
