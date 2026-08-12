using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;

namespace EventsRestApi.Interfaces
{
    public interface IEventService
    {
        PaginatedResult<EventResponseDto> Get(string? title, DateTime? from, DateTime? to, int page, int pageSize);
        EventResponseDto? GetById(Guid id);
        EventResponseDto Add(EventRequestDto addingEventDto);
        bool Update(Guid id, EventRequestDto newEventDto);
        bool Delete(Guid id);
        bool IsEventStillValid(Guid id);
    }
}
