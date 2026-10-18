using EventsRestApi.Data.Repositories.Interfaces;
using EventsRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.Data.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Event> GetAll()
        {
            return _context.Events.AsQueryable();
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
