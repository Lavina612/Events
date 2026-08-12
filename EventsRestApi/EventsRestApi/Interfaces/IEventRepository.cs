using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IEventRepository
    {
        IReadOnlyList<Event> Get();
        Event? GetById(Guid id);
        Event Add(Event addingEvent);
        bool Update(Event newEvent);
        bool Delete(Guid id);
    }
}
