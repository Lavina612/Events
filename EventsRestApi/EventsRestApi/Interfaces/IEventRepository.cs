using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IEventRepository
    {
        List<Event> Get();
        Event? GetById(Guid id);
        Event Add(Event addingEvent);
        bool Update(Guid id, Event newEvent);
        bool Delete(Guid id);
    }
}
