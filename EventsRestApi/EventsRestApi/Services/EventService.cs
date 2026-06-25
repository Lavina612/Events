using EventsRestApi.Dto;
using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private static readonly List<Event> events = [];

        public List<EventDto> GetAll(string? title, DateTime? from, DateTime? to)
        {
            var filteredEvents = events.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                filteredEvents = filteredEvents.Where(x => x.Title.Contains(title, StringComparison.CurrentCultureIgnoreCase));
            }

            if (from != null)
            {
                filteredEvents = filteredEvents.Where(x => x.StartAt >= from);
            }

            if (to != null)
            {
                filteredEvents = filteredEvents.Where(x => x.EndAt <= to);
            }

            return filteredEvents.Select(MapToEventDto).ToList();
        }

        public EventDto? GetById(int id)
        {
            var foundEvent = events.FirstOrDefault(x => x.Id == id);

            return foundEvent == null
                ? null
                : MapToEventDto(foundEvent);
        }

        public EventDto Add(EventDto addingEventDto)
        {
            addingEventDto.Id = events.Any() ? events.Max(x => x.Id) + 1 : 1;

            var addingEvent = MapToEvent(addingEventDto);

            events.Add(addingEvent);

            return MapToEventDto(addingEvent);
        }

        public bool Update(int id, EventDto newEventDto)
        {
            var updatingEvent = events.FirstOrDefault(x => x.Id == id);

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
            var deletingEvent = events.FirstOrDefault(x => x.Id == id);

            if (deletingEvent == null)
            {
                return false;
            }

            events.Remove(deletingEvent);

            return true;
        }

        private Event MapToEvent(EventDto eventDto)
        {
            return new Event
            {
                Id = eventDto.Id,
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartAt = eventDto.StartAt,
                EndAt = eventDto.EndAt
            };
        }

        private EventDto MapToEventDto(Event ev)
        {
            return new EventDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                StartAt = ev.StartAt,
                EndAt = ev.EndAt
            };
        }
    }
}
