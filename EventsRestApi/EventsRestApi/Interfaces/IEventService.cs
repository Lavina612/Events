using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Models;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        PaginatedResult<EventResponseDto> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize);
        EventResponseDto? GetDtoById(Guid id);
        EventResponseDto Add(EventRequestDto addingEventDto);
        bool Update(Guid id, EventRequestDto newEventDto);
        bool Delete(Guid id);
        Event? GetById(Guid id);
        bool Update(Event newEvent);
    }
}
