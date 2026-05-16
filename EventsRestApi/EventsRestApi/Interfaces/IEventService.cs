using EventsRestApi.Dto;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        List<EventDto> GetAll();
        EventDto? GetById(int id);
        EventDto Add(EventDto addingEventDto);
        bool Update(int id, EventDto newEventDto);
        bool Delete(int id);
    }
}
