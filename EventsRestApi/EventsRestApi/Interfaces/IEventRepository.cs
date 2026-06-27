using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IEventRepository
    {
        List<Event> Get();
        Event? GetById(int id);
        Event Add(Event addingEvent);
        bool Update(int id, Event newEvent);
        bool Delete(int id);
    }
}
