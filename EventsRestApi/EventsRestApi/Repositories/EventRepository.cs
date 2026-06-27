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

        public Event? GetById(int id)
        {
            return _events.FirstOrDefault(x => x.Id == id);
        }

        public Event Add(Event addingEvent)
        {
            addingEvent.Id = _events.Any() ? _events.Max(x => x.Id) + 1 : 1;

            _events.Add(addingEvent);

            return addingEvent;
        }

        public bool Update(int id, Event newEvent)
        {
            var updatingEvent = GetById(id);

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

        public bool Delete(int id)
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
