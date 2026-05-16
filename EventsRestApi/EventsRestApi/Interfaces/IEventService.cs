using EventsRestApi.Dto;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        List<EventDto> GetAll();
        EventDto GetById(int id);
        void Add(EventDto addingEvent);
        void Update(int id, EventDto newEvent);
        void Delete(int id);
    }
}
