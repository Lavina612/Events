using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Cache;
using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.EventService.Application.Services.Interfaces;
using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Models;
using System.Text.Json;

namespace EventsRestApi.Services.EventService.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventCacheService _eventCacheService;

        private readonly IEventRepository _eventRepository;

        public EventService(
            IEventCacheService eventCacheService,
            IEventRepository eventRepository)
        {
            _eventCacheService = eventCacheService;
            _eventRepository = eventRepository;
        }

        public async Task<PaginatedResult<Event>> GetPageAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _eventRepository.GetPageAsync(title, from, to, page, pageSize, cancellationToken);
        }

        public async Task<Event> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var cacheKey = $"event:{id}";
            var cachedEventJson = await _eventCacheService.GetAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedEventJson))
            {
                try
                {
                    var cachedEvent = JsonSerializer.Deserialize<Event>(cachedEventJson);

                    if (cachedEvent != null)
                    {
                        return cachedEvent;
                    }
                }
                catch { }
            }

            var @event = await _eventRepository.GetByIdAsync(id, cancellationToken);

            if (@event == null)
            {
                throw new NotFoundResourceException<Event>(id);
            }

            var serializedEvent = JsonSerializer.Serialize(@event);
            await _eventCacheService.SetAsync(cacheKey, serializedEvent, TimeSpan.FromMinutes(5));

            return @event;
        }

        public async Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken)
        {
            return await _eventRepository.AddAsync(addingEvent, cancellationToken);
        }

        public async Task<bool> UpdateAsync(Event updatingEvent, CancellationToken cancellationToken)
        {
            var result = await _eventRepository.UpdateAsync(updatingEvent, cancellationToken);

            var cacheKey = $"event:{updatingEvent.Id}";
            await _eventCacheService.DeleteAsync(cacheKey);

            return result;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _eventRepository.DeleteAsync(id, cancellationToken);

            var cacheKey = $"event:{id}";
            await _eventCacheService.DeleteAsync(cacheKey);

            return result;
        }
    }
}
