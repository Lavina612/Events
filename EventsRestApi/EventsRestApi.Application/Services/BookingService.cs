using EventsRestApi.Application.Exceptions;
using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Application.Services.Interfaces;
using EventsRestApi.Domain.Exceptions;
using EventsRestApi.Domain.Models.Entities;

namespace EventsRestApi.Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly IBookingRepository _bookingRepository;

        private readonly IEventService _eventService;

        private readonly TimeProvider _timeProvider;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventService eventService,
            TimeProvider timeProvider)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
            _timeProvider = timeProvider;
        }

        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, int requestedSeats, CancellationToken cancellationToken)
        {
            Booking? addedBooking = null;

            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var foundEvent = await _eventService.GetByIdAsync(eventId, cancellationToken);

                if (foundEvent == null)
                {
                    throw new NotFoundEventException(eventId);
                }

                if (!foundEvent.IsStillActual(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    throw new FinishedEventException(eventId, foundEvent.EndAt);
                }

                if (!foundEvent.TryReserveSeats(requestedSeats))
                {
                    throw new NoAvailableSeatsException(eventId, requestedSeats, foundEvent.AvailableSeats);
                }

                addedBooking = await _bookingRepository.AddAsync(eventId, userId, requestedSeats, cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return addedBooking;
        }
    }
}
