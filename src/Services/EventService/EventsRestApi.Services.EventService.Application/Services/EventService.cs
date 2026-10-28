using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Cache;
using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.EventService.Application.Models;
using EventsRestApi.Services.EventService.Application.Services.Interfaces;
using EventsRestApi.Services.EventService.Application.Settings;
using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventsRestApi.Services.EventService.Application.Services
{
    public class EventService : IEventService
    {
        private readonly ICacheService _eventCacheService;

        private readonly IEventRepository _eventRepository;

        private readonly CacheSettings _cacheSettings;

        private readonly ILogger<EventService> _logger;

        public EventService(
            ICacheService eventCacheService,
            IEventRepository eventRepository,
            IOptions<CacheSettings> cacheSettings,
            ILogger<EventService> logger)
        {
            _eventCacheService = eventCacheService;
            _eventRepository = eventRepository;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        public async Task<PaginatedResult<Event>> GetPageAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _eventRepository.GetPageAsync(title, from, to, page, pageSize, cancellationToken);
        }

        public async Task<List<Event>> GetTop10PopularAsync(CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.GetTop10Events;
            var cachedTopEventsJson = await _eventCacheService.GetAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedTopEventsJson))
            {
                try
                {
                    var cachedEvents = JsonSerializer.Deserialize<List<Event>>(cachedTopEventsJson);

                    if (cachedEvents != null)
                    {
                        return cachedEvents;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Ошибка при попытке десериализовать закэшированный объект с Key: {CacheKey}.",
                        cacheKey);
                }
            }

            var topEvents = await _eventRepository.GetTop10PopularAsync(cancellationToken);

            var serializedEvents = JsonSerializer.Serialize(topEvents);
            await _eventCacheService.SetAsync(cacheKey, serializedEvents, TimeSpan.FromMinutes(_cacheSettings.TopEventsTtlMinutes));

            return topEvents;
        }

        public async Task<Event> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var cacheKey = $"{CacheKeys.GetEventById}{id}";
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
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Ошибка при попытке десериализовать закэшированный объект с Key: {CacheKey}.",
                        cacheKey);
                }
            }

            var @event = await _eventRepository.GetByIdAsync(id, cancellationToken);

            if (@event == null)
            {
                throw new NotFoundResourceException<Event>(id);
            }

            var serializedEvent = JsonSerializer.Serialize(@event);
            await _eventCacheService.SetAsync(cacheKey, serializedEvent, TimeSpan.FromMinutes(_cacheSettings.EventByIdTtlMinutes));

            return @event;
        }

        public async Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken)
        {
            return await _eventRepository.AddAsync(addingEvent, cancellationToken);
        }

        public async Task<bool> UpdateAsync(Event updatingEvent, CancellationToken cancellationToken)
        {
            var result = await _eventRepository.UpdateAsync(updatingEvent, cancellationToken);

            var cacheKey = $"{CacheKeys.GetEventById}{updatingEvent.Id}";
            await _eventCacheService.DeleteAsync(cacheKey);

            return result;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _eventRepository.DeleteAsync(id, cancellationToken);

            var cacheKey = $"{CacheKeys.GetEventById}{id}";
            await _eventCacheService.DeleteAsync(cacheKey);

            return result;
        }
    }
}
