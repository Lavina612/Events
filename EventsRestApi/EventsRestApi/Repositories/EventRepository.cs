using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Repositories
{
    public class EventRepository : IEventRepository
    {
        private static readonly List<Event> _events = [];

        public List<Event> Get()
        {
            return _events;
        }

        public Event? GetById(Guid id)
        {
            return _events.FirstOrDefault(x => x.Id == id);
        }

        public Event Add(Event addingEvent)
        {
            addingEvent.Id = Guid.NewGuid();

            _events.Add(addingEvent);

            return addingEvent;
        }

        public bool Update(Event newEvent)
        {
            var updatingEvent = GetById(newEvent.Id);

            if (updatingEvent == null)
            {
                return false;
            }

            updatingEvent.Title = newEvent.Title;
            updatingEvent.Description = newEvent.Description;
            updatingEvent.StartAt = newEvent.StartAt;
            updatingEvent.EndAt = newEvent.EndAt;

            return true;
        }

        public bool Delete(Guid id)
        {
            var deletingEvent = GetById(id);

            if (deletingEvent == null)
            {
                return false;
            }

            _events.Remove(deletingEvent);

            return true;
        }
    }
}
