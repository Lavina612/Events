using EventsRestApi.Dto.Response;
using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        PaginatedResult<Event> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize);
        Event? GetById(Guid id);
        Event Add(Event addingEvent);
        bool Update(Event updatingEvent);
        bool Delete(Guid id);
    }
}
