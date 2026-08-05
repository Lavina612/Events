using EventsRestApi.Dto;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        PaginatedResult<EventDto> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize);
        EventDto? GetById(Guid id);
        EventDto Add(EventDto addingEventDto);
        bool Update(Guid id, EventDto newEventDto);
        bool Delete(Guid id);
    }
}
