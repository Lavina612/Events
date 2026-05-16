using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Services
{
    public class EventService : IEventService
    {
        private static List<Event> events = [];

        public List<Event> GetAll()
        {
            return events;
        }

        public Event GetById(int id)
        {
            var foundEvent = events.FirstOrDefault(x => x.Id == id);

            if (foundEvent == null)
            {
                throw new ArgumentException($"Событие с id:{id} не найдено.");
            }

            return foundEvent;
        }

        public void Add(Event addingEvent)
        {
            events.Add(addingEvent);
        }

        public void Update(int id, Event newEvent)
        {
            var updatingEvent = GetById(id);

            updatingEvent.Title = newEvent.Title;
            updatingEvent.Description = newEvent.Description;
            updatingEvent.StartAt = newEvent.StartAt;
            updatingEvent.EndAt = newEvent.EndAt;
        }

        public void Delete(int id)
        {
            var deletingEvent = GetById(id);

            events.Remove(deletingEvent);
        }
    }
}
