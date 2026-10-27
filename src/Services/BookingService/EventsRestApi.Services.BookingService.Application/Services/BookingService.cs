using EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.MessageBroker;
using EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.BookingService.Application.Services.Interfaces;
using EventsRestApi.Services.BookingService.Application.Settings;
using EventsRestApi.Services.BookingService.Domain.Exceptions;
using EventsRestApi.Services.BookingService.Domain.Models.Entities;
using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.MessageBroker;
using EventsRestApi.Services.SharedContracts.Models;
using Microsoft.Extensions.Options;

namespace EventsRestApi.Services.BookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly IBookingRepository _bookingRepository;

        private readonly IEventPublisher _eventPublisher;

        private readonly BookingSettings _bookingSettings;

        private readonly TimeProvider _timeProvider;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventPublisher eventPublisher,
            IOptions<BookingSettings> bookingSettings,
            TimeProvider timeProvider)
        {
            _bookingRepository = bookingRepository;
            _eventPublisher = eventPublisher;
            _bookingSettings = bookingSettings.Value;
            _timeProvider = timeProvider;
        }

        public async Task<IReadOnlyList<Booking>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetAllByUserId(userId, cancellationToken);
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, UserRole userRole, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                throw new NotFoundResourceException<Booking>(bookingId);
            }

            if (userRole != UserRole.Admin && booking.UserId != userId)
            {
                throw new ForbiddenException();
            }

            return booking;
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, int requestedSeats, CancellationToken cancellationToken)
        {
            Booking? addedBooking = null;

            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var activeBookingsCount = await _bookingRepository.GetActiveBookingsCountAsync(userId, cancellationToken);

                if (activeBookingsCount >= _bookingSettings.ActiveBookingLimit)
                {
                    throw new ExceedingActiveBookingLimitException(userId, _bookingSettings.ActiveBookingLimit);
                }

                addedBooking = await _bookingRepository.AddAsync(eventId, userId, requestedSeats, cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return addedBooking;
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid userId, UserRole userRole, CancellationToken cancellationToken)
        {
            var booking = await GetBookingByIdAsync(bookingId, userId, userRole, cancellationToken);

            if (booking.Status == BookingStatus.Cancelled)
            {
                return;
            }

            var bookingWasConfirmedBeforeCancellation = booking.Status == BookingStatus.Confirmed;

            booking.Cancel(_timeProvider.GetUtcNow().UtcDateTime);
            await _bookingRepository.SaveContextAsync(cancellationToken);

            if (bookingWasConfirmedBeforeCancellation)
            {
                var bookingCancelledEvent = new BookingCancelledEvent(
                    BookingId: booking.Id,
                    EventId: booking.EventId,
                    UserId: booking.UserId,
                    BookedSeats: booking.BookedSeats,
                    CancelledAt: _timeProvider.GetUtcNow().DateTime
                );

                await _eventPublisher.PublishAsync(
                    topic: BookingTopics.BookingCancelled,
                    messageKey: booking.EventId.ToString(),
                    message: bookingCancelledEvent,
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
