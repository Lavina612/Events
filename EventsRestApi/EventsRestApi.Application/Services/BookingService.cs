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

        private readonly IUserService _userService;

        private readonly TimeProvider _timeProvider;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventService eventService,
            IUserService userService,
            TimeProvider timeProvider)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
            _userService = userService;
            _timeProvider = timeProvider;
        }

        public async Task<IReadOnlyList<Booking>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetAllByUserId(userId, cancellationToken);
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                throw new NotFoundResourceException<Booking>(bookingId);
            }

            await EnsureUserHasAccessAsync(userId, booking.UserId, cancellationToken);

            return booking;
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, int requestedSeats, CancellationToken cancellationToken)
        {
            await _userService.EnsureCanMakeBookingAsync(userId, cancellationToken);

            Booking? addedBooking = null;

            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var foundEvent = await _eventService.GetByIdAsync(eventId, cancellationToken);

                if (foundEvent == null)
                {
                    throw new NotFoundResourceException<Event>(eventId);
                }

                if (foundEvent.IsFinished(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    throw new FinishedEventException(eventId, foundEvent.EndAt);
                }

                if (foundEvent.IsStarted(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    throw new StartedEventException(eventId, foundEvent.StartAt);
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

        public async Task CancelBookingAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken)
        {
            var booking = await GetBookingByIdAsync(bookingId, userId, cancellationToken);

            if (booking.Status == BookingStatus.Cancelled)
            {
                return;
            }

            booking.Cancel(_timeProvider.GetUtcNow().UtcDateTime);

            var foundEvent = await _eventService.GetByIdAsync(booking.EventId, cancellationToken);

            if (foundEvent != null)
            {
                foundEvent.ReleaseSeats(booking.BookedSeats);
            }

            await _bookingRepository.SaveContextAsync(cancellationToken);
        }

        private async Task EnsureUserHasAccessAsync(Guid userId, Guid bookingUserId, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);

            if (user.Role != UserRole.Admin && bookingUserId != user.Id)
            {
                throw new ForbiddenException(user.Id);
            }
        }
    }
}
