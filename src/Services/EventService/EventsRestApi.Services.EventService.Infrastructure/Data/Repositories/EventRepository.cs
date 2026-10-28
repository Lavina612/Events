using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Services.EventService.Infrastructure.Data.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly EventDbContext _context;

        public EventRepository(EventDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Event>> GetPageAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _context.Events.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{title}%"));
            }

            if (from != null)
            {
                var utcFrom = from.Value.ToUniversalTime();
                query = query.Where(x => x.StartAt >= utcFrom);
            }

            if (to != null)
            {
                var utcTo = to.Value.ToUniversalTime();
                query = query.Where(x => x.EndAt <= utcTo);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var filteredEventsForPage = await query
                .OrderBy(x => x.StartAt)
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Event>(
                filteredEventsForPage,
                totalCount,
                page,
                pageSize);
        }

        public async Task<List<Event>> GetTop10PopularAsync(CancellationToken cancellationToken)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(x => x.TotalSeats > 0)
                .OrderByDescending(x => ((double)(x.TotalSeats - x.AvailableSeats)) / x.TotalSeats)
                .Take(10)
                .ToListAsync(cancellationToken);
        }

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Event> AddAsync(Event addingEvent, CancellationToken cancellationToken)
        {
            Event realAddingEvent;

            if (addingEvent.Id == Guid.Empty)
            {
                realAddingEvent = new Event(
                    Guid.NewGuid(),
                    addingEvent.Title,
                    addingEvent.Description,
                    addingEvent.StartAt,
                    addingEvent.EndAt,
                    addingEvent.TotalSeats);
            }
            else
            {
                realAddingEvent = addingEvent;
            }

            await _context.Events.AddAsync(realAddingEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return realAddingEvent;
        }

        public async Task<bool> UpdateAsync(Event newEvent, CancellationToken cancellationToken)
        {
            var updatingEvent = await GetByIdAsync(newEvent.Id, cancellationToken);

            if (updatingEvent == null)
            {
                return false;
            }

            updatingEvent.Title = newEvent.Title;
            updatingEvent.Description = newEvent.Description;
            updatingEvent.SetDates(newEvent.StartAt, newEvent.EndAt);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var deletingEvent = await GetByIdAsync(id, cancellationToken);

            if (deletingEvent == null)
            {
                return false;
            }

            _context.Events.Remove(deletingEvent);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task SaveContextAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
