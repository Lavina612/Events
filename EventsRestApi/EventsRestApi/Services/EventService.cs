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

        public Event? GetById(int id)
        {
            return events.FirstOrDefault(x => x.Id == id);
        }

        public void Add(Event addingEvent) 
        {
            events.Add(addingEvent);
        }

        public void Update(int id, Event newEvent)
        {
            var updatingEvent = events.FirstOrDefault(x => x.Id == id);

            if (updatingEvent != null)
            {
                updatingEvent.Title = newEvent.Title;
                updatingEvent.Description = newEvent.Description;
                updatingEvent.StartAt = newEvent.StartAt;
                updatingEvent.EndAt = newEvent.EndAt;
            }
        }

        public void Delete(Event deletingEvent)
        {
            events.Remove(deletingEvent);
        }
    }
}
