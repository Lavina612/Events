using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.EventService.Application.Services.Interfaces;
using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Models;

namespace EventsRestApi.Services.EventService.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<PaginatedResult<Event>> GetPageAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _eventRepository.GetPageAsync(title, from, to, page, pageSize, cancellationToken);
        }

        public async Task<Event> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(id, cancellationToken);

            if (@event == null)
            {
                throw new NotFoundResourceException<Event>(id);
            }

            return @event;
        }

        public async Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken)
        {
            return await _eventRepository.AddAsync(addingEvent, cancellationToken);
        }

        public async Task<bool> UpdateAsync(Event updatingEvent, CancellationToken cancellationToken)
        {
            return await _eventRepository.UpdateAsync(updatingEvent, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _eventRepository.DeleteAsync(id, cancellationToken);
        }
    }
}
