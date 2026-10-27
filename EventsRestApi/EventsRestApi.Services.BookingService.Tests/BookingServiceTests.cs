using EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.MessageBroker;
using EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.BookingService.Application.Services.Interfaces;
using EventsRestApi.Services.BookingService.Application.Settings;
using EventsRestApi.Services.BookingService.Domain.Exceptions;
using EventsRestApi.Services.BookingService.Domain.Models.Entities;
using EventsRestApi.Services.BookingService.Infrastructure.Data;
using EventsRestApi.Services.BookingService.Infrastructure.Data.Repositories;
using EventsRestApi.Services.BookingService.Infrastructure.MessageBroker;
using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace EventsRestApi.Services.BookingService.Tests
{
    public class BookingServiceTests : IDisposable
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 01, 09, 01, 01, TimeSpan.Zero);
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly ServiceProvider _serviceProvider;
        private readonly ServiceCollection _services;
        private readonly BookingDbContext _context;
        private readonly IBookingService _bookingService;

        public BookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            _services = new ServiceCollection();
            _services.AddDbContext<BookingDbContext>(options => options.UseInMemoryDatabase(dbName));

            _services.AddScoped<IBookingRepository, BookingRepository>();
            _services.AddScoped<IBookingService, Application.Services.BookingService>();
            _services.AddScoped<IEventPublisher, KafkaEventPublisher>();

            _services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            _services.AddSingleton<IConfiguration>(configuration);

            _services.Configure<BookingBackgroundServiceSettings>(
                configuration.GetSection(BookingBackgroundServiceSettings.SectionName));

            var serviceProvider = _services.BuildServiceProvider();

            _fakeTimeProvider = new FakeTimeProvider();
            _fakeTimeProvider.SetUtcNow(fixedTime);
            _services.AddSingleton<TimeProvider>(_fakeTimeProvider);

            _serviceProvider = _services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<BookingDbContext>();
            _bookingService = _serviceProvider.GetRequiredService<IBookingService>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _serviceProvider.Dispose();
        }

        #region GetBookingByIdAsync Tests

        [Fact]
        public async Task GetBookingByIdAsync_WithExistingId_ReturnBooking()
        {
            /*---ARRANGE---*/
            var booking = await AddBookingToContext(1, 1, 1, 1, fixedTime.UtcDateTime);
            await _context.SaveChangesAsync();

            /*---ACT---*/
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, booking.UserId, UserRole.User, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(booking, result, true);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ACT & ASSERT---*/
            await Assert.ThrowsAnyAsync<NotFoundResourceException<Booking>>(async () =>
                await _bookingService.GetBookingByIdAsync(Guid.Empty, new Guid($"A0000000-0000-0000-0000-000000000001"), UserRole.User, CancellationToken.None));
        }

        [Fact]
        public async Task GetBookingByIdAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            var booking = await AddBookingToContext(1, 1, 1, 1, fixedTime.UtcDateTime);
            await _context.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _bookingService.GetBookingByIdAsync(booking.Id, booking.UserId, UserRole.User, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        #endregion

        #region CreateBookingAsync Tests

        [Fact]
        public async Task CreateBookingAsync_WithExistingValidEventWithEnoughAvailableSeats_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var booking = await AddBookingToContext(1, 1, 1, 1, createdAt);
            await _context.SaveChangesAsync();

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(booking.EventId, booking.UserId, booking.BookedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(booking, options => options.Excluding(x => x.Id));
        }

        [Fact]
        public async Task CreateBookingAsync_ForEventThatHasAnotherBooking_ReturnNewCreatedBooking()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var requestedSeats = 1;
            var eventId = new Guid($"E0000000-0000-0000-0000-000000000001");
            var userId = new Guid($"A0000000-0000-0000-0000-000000000001");

            /*---ACT---*/
            var firstResult = await _bookingService.CreateBookingAsync(eventId, userId, requestedSeats, CancellationToken.None);
            var secondResult = await _bookingService.CreateBookingAsync(eventId, userId, requestedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.NotEqual(firstResult.Id, secondResult.Id);
        }

        [Fact]
        public async Task CreateBookingAsync_BookSeatAfterRelease_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var booking = await AddBookingToContext(1, 1, 1, 1, createdAt);
            await _context.SaveChangesAsync();

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(booking.EventId, booking.UserId, booking.BookedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(booking, options => options.Excluding(x => x.Id));
        }

        [Fact]
        public async Task CreateBookingAsync_TryCreateAfterActiveBookingsLimitExceeded_ThrowExceedingActiveBookingLimitException()
        {
            /*---ARRANGE---*/
            var currentSettings = _serviceProvider.GetRequiredService<IOptions<BookingSettings>>().Value;
            currentSettings.ActiveBookingLimit = 1;

            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var requestedSeats = 1;

            var booking = await AddBookingToContext(1, 1, 1, requestedSeats, createdAt, BookingStatus.Confirmed);
            await _context.SaveChangesAsync();

            /*---ACT & ASSERT---*/
            var result = await Assert.ThrowsAnyAsync<ExceedingActiveBookingLimitException>(async () =>
                await _bookingService.CreateBookingAsync(booking.EventId, booking.UserId, requestedSeats, CancellationToken.None));
        }

        [Fact]
        public async Task CreateBookingAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _bookingService.CreateBookingAsync(Guid.Empty, Guid.Empty, 0, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        #endregion

        private async Task<Booking> AddBookingToContext(
            int numberBooking,
            int numberEvent,
            int numberUser,
            int bookedSeats,
            DateTime createdAt,
            BookingStatus status = BookingStatus.Pending)
        {
            var booking = new Booking(
                new Guid($"B0000000-0000-0000-0000-00000000000{numberBooking}"),
                new Guid($"E0000000-0000-0000-0000-00000000000{numberEvent}"),
                new Guid($"A0000000-0000-0000-0000-00000000000{numberUser}"),
                status,
                bookedSeats,
                createdAt);

            await _context.Bookings.AddAsync(booking);

            return booking;
        }
    }
}
