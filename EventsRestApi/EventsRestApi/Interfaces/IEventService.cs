using EventsRestApi.Dto;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        PaginatedResult<EventDto> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize);
        EventDto? GetById(int id);
        EventDto Add(EventDto addingEventDto);
        bool Update(int id, EventDto newEventDto);
        bool Delete(int id);
    }
}
