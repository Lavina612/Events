using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        List<Event> GetAll();
        Event GetById(int id);
        void Add(Event addingEvent);
        void Update(int id, Event newEvent);
        void Delete(int id);
    }
}
