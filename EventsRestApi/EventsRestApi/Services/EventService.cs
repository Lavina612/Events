using EventsRestApi.Dto;
using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private static List<Event> events = [];

        public List<EventDto> GetAll()
        {
            return events.Select(MapToEventDto).ToList();
        }

        public EventDto GetById(int id)
        {
            var foundEvent = GetEventById(id);

            return MapToEventDto(foundEvent);
        }

        public void Add(EventDto addingEventDto)
        {
            CheckUniqueId(addingEventDto.Id);

            events.Add(MapToEvent(addingEventDto));
        }

        public void Update(int id, EventDto newEventDto)
        {
            var updatingEvent = GetEventById(id);

            CheckUniqueId(newEventDto.Id);

            var newEvent = MapToEvent(newEventDto);

            updatingEvent = newEvent;
        }

        public void Delete(int id)
        {
            var deletingEvent = GetEventById(id);

            events.Remove(deletingEvent);
        }

        private Event GetEventById(int id)
        {
            var foundEvent = events.FirstOrDefault(x => x.Id == id);

            if (foundEvent == null)
            {
                throw new ArgumentException($"Событие с id:{id} не найдено.");
            }

            return foundEvent;
        }

        private void CheckUniqueId(int id)
        {
            if (events.FirstOrDefault(x => x.Id == id) != null)
            {
                throw new ArgumentException($"Событий с id:{id} уже существует.");
            }
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
