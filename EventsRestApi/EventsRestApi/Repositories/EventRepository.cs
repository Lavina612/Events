using EventsRestApi.Interfaces;
using EventsRestApi.Models;

namespace EventsRestApi.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly List<Event> _events = [];

        public IReadOnlyList<Event> Get()
        {
            return _events.AsReadOnly();
        }

        public Event? GetById(Guid id)
        {
            return _events.FirstOrDefault(x => x.Id == id);
        }

        public Event Add(Event addingEvent)
        {
            Event realAddingEvent;

            if (addingEvent.Id == Guid.Empty)
            {
                realAddingEvent = new Event(
                    Guid.NewGuid(),
                    addingEvent.Title,
                    addingEvent.Description,
                    addingEvent.StartAt,
                    addingEvent.EndAt,
                    addingEvent.TotalSeats);
            }
            else
            {
                realAddingEvent = addingEvent;
            }

            _events.Add(realAddingEvent);

            return realAddingEvent;
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
