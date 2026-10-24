using EventsRestApi.Application.Exceptions;
using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Application.InterfacesForInfrastructure.Services;
using EventsRestApi.Application.Services;
using EventsRestApi.Application.Services.Interfaces;
using EventsRestApi.Application.Settings;
using EventsRestApi.Domain.Exceptions;
using EventsRestApi.Domain.Models.Entities;
using EventsRestApi.Infrastructure.Data;
using EventsRestApi.Infrastructure.Data.Repositories;
using EventsRestApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace EventsRestApi.Tests
{
    public class BookingServiceTests : IDisposable
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 01, 09, 01, 01, TimeSpan.Zero);
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly ServiceProvider _serviceProvider;
        private readonly ServiceCollection _services;
        private readonly AppDbContext _context;
        private readonly IBookingService _bookingService;

        public BookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            _services = new ServiceCollection();
            _services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));

            _services.AddScoped<IEventRepository, EventRepository>();
            _services.AddScoped<IBookingRepository, BookingRepository>();
            _services.AddScoped<IUserRepository, UserRepository>();

            _services.AddScoped<ITokenService, TokenService>();

            _services.AddScoped<IEventService, EventService>();
            _services.AddScoped<IBookingService, BookingService>();
            _services.AddScoped<IPasswordService, PasswordService>();
            _services.AddScoped<IUserService, UserService>();

            _services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            _services.Configure<BookingBackgroundServiceSettings>(
                configuration.GetSection(BookingBackgroundServiceSettings.SectionName));

            var serviceProvider = _services.BuildServiceProvider();

            _fakeTimeProvider = new FakeTimeProvider();
            _fakeTimeProvider.SetUtcNow(fixedTime);
            _services.AddSingleton<TimeProvider>(_fakeTimeProvider);

            _serviceProvider = _services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<AppDbContext>();
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
            var userNumber = 1;

            var user = await AddUserToContextAsync(userNumber);
            var booking = await AddBookingToContext(1, 1, userNumber, 1, fixedTime.UtcDateTime);
            await _context.SaveChangesAsync();

            /*---ACT---*/
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, user.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(booking, result, true);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            var bookingId = Guid.Empty;

            var user = await AddUserToContextAsync(1);
            await _context.SaveChangesAsync();

            /*---ACT & ASSERT---*/
            await Assert.ThrowsAnyAsync<NotFoundResourceException<Booking>>(async () =>
                await _bookingService.GetBookingByIdAsync(bookingId, user.Id, CancellationToken.None));
        }

        [Fact]
        public async Task GetBookingByIdAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            var userNumber = 1;

            var user = await AddUserToContextAsync(userNumber);
            var booking = await AddBookingToContext(1, 1, userNumber, 1, fixedTime.UtcDateTime);
            await _context.SaveChangesAsync();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _bookingService.GetBookingByIdAsync(booking.Id, user.Id, cts.Token));

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
            var eventNumber = 1;
            var userNumber = 1;

            var user = await AddUserToContextAsync(userNumber);
            var @event = await AddEventToContextAsync(eventNumber, createdAt.AddDays(5), createdAt.AddDays(10), 1);
            var booking = await AddBookingToContext(1, eventNumber, userNumber, 1, createdAt);
            await _context.SaveChangesAsync();

            var availableSeatsAfterBooking = @event.AvailableSeats - booking.BookedSeats;

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(@event.Id, user.Id, booking.BookedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(booking, options => options.Excluding(x => x.Id));
            Assert.Equal(availableSeatsAfterBooking, @event.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ForEventThatHasAnotherBooking_ReturnNewCreatedBooking()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var requestedSeats = 1;
            var successfulBookingsCount = 2;

            var user = await AddUserToContextAsync(1);
            var @event = await AddEventToContextAsync(1, createdAt.AddDays(5), createdAt.AddDays(10), 2);
            await _context.SaveChangesAsync();

            var availableSeatsAfterBooking = @event.AvailableSeats - requestedSeats * successfulBookingsCount;

            /*---ACT---*/
            var firstResult = await _bookingService.CreateBookingAsync(@event.Id, user.Id, requestedSeats, CancellationToken.None);
            var secondResult = await _bookingService.CreateBookingAsync(@event.Id, user.Id, requestedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.NotEqual(firstResult.Id, secondResult.Id);
            Assert.Equal(availableSeatsAfterBooking, @event.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_BookSeatAfterRelease_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var eventNumber = 1;
            var userNumber = 1;

            var user = await AddUserToContextAsync(userNumber);
            var @event = await AddEventToContextAsync(eventNumber, createdAt.AddDays(5), createdAt.AddDays(10), 1);
            var booking = await AddBookingToContext(1, eventNumber, userNumber, 1, createdAt);
            await _context.SaveChangesAsync();

            /*---ACT---*/
            @event.TryReserveSeats(1);
            @event.ReleaseSeats(1);
            var availableSeatsAfterBooking = @event.AvailableSeats - booking.BookedSeats;

            var result = await _bookingService.CreateBookingAsync(@event.Id, user.Id, booking.BookedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(booking, options => options.Excluding(x => x.Id));
            Assert.Equal(availableSeatsAfterBooking, @event.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_TryCreateAfterActiveBookingsLimitExceeded_ThrowExceedingActiveBookingLimitException()
        {
            /*---ARRANGE---*/
            var currentSettings = _serviceProvider.GetRequiredService<IOptions<UserSettings>>().Value;
            currentSettings.ActiveBookingLimit = 1;

            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var requestedSeats = 1;
            var successfulBookingsCount = 1;
            var eventNumber = 1;
            var userNumber = 1;

            var user = await AddUserToContextAsync(userNumber);
            var @event = await AddEventToContextAsync(eventNumber, createdAt.AddDays(5), createdAt.AddDays(10), 1);
            var booking = await AddBookingToContext(1, eventNumber, userNumber, requestedSeats, createdAt, BookingStatus.Confirmed);
            await _context.SaveChangesAsync();

            var availableSeatsAfterBooking = @event.AvailableSeats - requestedSeats * successfulBookingsCount;

            /*---ACT & ASSERT---*/
            var result = await Assert.ThrowsAnyAsync<ExceedingActiveBookingLimitException>(async () =>
                await _bookingService.CreateBookingAsync(@event.Id, user.Id, requestedSeats, CancellationToken.None));
        }

        [Fact]
        public async Task CreateBookingAsync_AnotherUserExceededActiveBookingsCount_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            var currentSettings = _serviceProvider.GetRequiredService<IOptions<UserSettings>>().Value;
            currentSettings.ActiveBookingLimit = 1;

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var requestedSeats = 1;
            var successfulBookingsCount = 1;
            var eventNumber = 1;
            var user1Number = 1;
            var user2Number = 2;

            var user1 = await AddUserToContextAsync(user1Number);
            var user2 = await AddUserToContextAsync(user2Number);
            var @event = await AddEventToContextAsync(eventNumber, createdAt.AddDays(5), createdAt.AddDays(10), 2);
            var booking = await AddBookingToContext(1, eventNumber, user1Number, requestedSeats, createdAt, BookingStatus.Confirmed);
            await _context.SaveChangesAsync();

            var expectedBooking = await AddBookingToContext(2, eventNumber, user2Number, requestedSeats, createdAt, BookingStatus.Pending);

            var availableSeatsAfterBooking = @event.AvailableSeats - requestedSeats * successfulBookingsCount;

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(@event.Id, user2.Id, booking.BookedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(expectedBooking, options =>
                options.Excluding(x => x.Id)
                    .Excluding(x => x.Event)
                    .Excluding(x => x.User));
            Assert.Equal(availableSeatsAfterBooking, @event.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_TryCreateAfterNoMoreAvailableSeats_ThrowNoAvailableSeatsException()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var requestedSeats = 1;
            var successfulBookingsCount = 1;
            var eventNumber = 1;
            var userNumber = 1;

            var user = await AddUserToContextAsync(userNumber);
            var @event = await AddEventToContextAsync(eventNumber, createdAt.AddDays(5), createdAt.AddDays(10), 1);
            var booking = await AddBookingToContext(1, eventNumber, userNumber, requestedSeats, createdAt);
            await _context.SaveChangesAsync();

            var availableSeatsAfterBooking = @event.AvailableSeats - requestedSeats * successfulBookingsCount;

            /*---ACT & ASSERT---*/
            var firstResult = await _bookingService.CreateBookingAsync(@event.Id, user.Id, requestedSeats, CancellationToken.None);

            var secondResultException = await Assert.ThrowsAnyAsync<NoAvailableSeatsException>(async () =>
                await _bookingService.CreateBookingAsync(@event.Id, user.Id, requestedSeats, CancellationToken.None));

            Assert.NotEqual(Guid.Empty, firstResult.Id);
            firstResult.Should().BeEquivalentTo(booking, options => options.Excluding(x => x.Id));
            Assert.Equal(availableSeatsAfterBooking, @event.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_WithNonExistentEvent_ThrowNotFoundEventException()
        {
            /*---ARRANGE---*/
            var eventId = Guid.Empty;

            var user = await AddUserToContextAsync(1);
            await _context.SaveChangesAsync();

            /*---ACT & ASSERT---*/
            await Assert.ThrowsAnyAsync<NotFoundResourceException<Event>>(async () =>
                await _bookingService.CreateBookingAsync(eventId, user.Id, It.IsAny<int>(), CancellationToken.None));
        }

        [Fact]
        public async Task CreateBookingAsync_WithAlreadyStartedEvent_ThrowStartedEventException()
        {
            /*---ARRANGE---*/
            //Дата startAt высчитывается относительно fixedTime: 2026.06.06 09:06:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(1).UtcDateTime;
            var endAt = startAt.AddDays(4);

            var user = await AddUserToContextAsync(1);
            var @event = await AddEventToContextAsync(1, startAt, endAt, 1);
            await _context.SaveChangesAsync();

            /*---ACT & ASSERT---*/
            await Assert.ThrowsAnyAsync<StartedEventException>(async () =>
                await _bookingService.CreateBookingAsync(@event.Id, user.Id, 1, CancellationToken.None));
        }

        [Fact]
        public async Task CreateBookingAsync_WithFinishedEvent_ThrowFinishedEventException()
        {
            /*---ARRANGE---*/
            //Дата startAt высчитывается относительно fixedTime: 2026.06.06 09:06:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(1).UtcDateTime;
            var endAt = startAt.AddDays(1);

            var user = await AddUserToContextAsync(1);
            var @event = await AddEventToContextAsync(1, startAt, endAt, 1);
            await _context.SaveChangesAsync();

            /*---ACT & ASSERT---*/
            await Assert.ThrowsAnyAsync<FinishedEventException>(async () =>
                await _bookingService.CreateBookingAsync(@event.Id, user.Id, 1, CancellationToken.None));
        }

        [Fact]
        public async Task CreateBookingAsync_WithRequestedSeatsMoreThanAvailable_ThrowNoAvailableSeatsException()
        {
            /*---ARRANGE---*/
            //Дата startAt высчитывается относительно fixedTime: 2026.06.06 09:06:00
            var startAt = fixedTime.AddDays(1).AddMinutes(1).UtcDateTime;
            var endAt = startAt.AddDays(5);

            var user = await AddUserToContextAsync(1);
            var @event = await AddEventToContextAsync(1, startAt, endAt, 1);
            await _context.SaveChangesAsync();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<NoAvailableSeatsException>(async () =>
                await _bookingService.CreateBookingAsync(@event.Id, user.Id, 5, CancellationToken.None));
        }

        [Fact]
        public async Task CreateBookingAsync_ManyConcurrentAttemptToCreateBooking_NoOverbookingForEvent()
        {
            /*---ARRANGE---*/
            var requestedSeats = 1;
            var eventTotalSeats = 5;
            var bookingRequestsCount = 20;
            var uniqueCreatedBookingsId = new HashSet<Guid>();

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var user = await AddUserToContextAsync(1);
            var @event = await AddEventToContextAsync(1, createdAt.AddDays(5), createdAt.AddDays(10), eventTotalSeats);
            await _context.SaveChangesAsync();

            /*---ACT---*/
            var tasks = new List<Task<Booking>>();

            for (int i = 0; i < bookingRequestsCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                        return await bookingService.CreateBookingAsync(@event.Id, user.Id, requestedSeats, CancellationToken.None);
                    }));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch { }

            /*---ASSERT---*/
            var successfulTasks = tasks.Where(x => x.Status == TaskStatus.RanToCompletion).ToList();
            var failedTasks = tasks.Where(x => x.Status == TaskStatus.Faulted).ToList();

            Assert.Equal(eventTotalSeats, successfulTasks.Count);

            foreach (var task in successfulTasks)
            {
                var result = await task;

                Assert.NotNull(result);

                uniqueCreatedBookingsId.Add(result.Id);
            }

            Assert.Equal(eventTotalSeats, uniqueCreatedBookingsId.Count);

            Assert.Equal(bookingRequestsCount - eventTotalSeats, failedTasks.Count);

            foreach (var task in failedTasks)
            {
                var exception = task.Exception?.InnerException;

                Assert.NotNull(exception);
                Assert.IsType<NoAvailableSeatsException>(exception);
            }

            var updatedEvent = _context.Events.AsNoTracking().First();
            Assert.Equal(@event.Id, updatedEvent.Id);
            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_CheckParallelTasksCount_SemaphoreIsSetCorrectly()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;
            var user = await AddUserToContextAsync(1);
            var @event = await AddEventToContextAsync(1, createdAt.AddDays(-1), createdAt.AddDays(1), 5);
            await _context.SaveChangesAsync();

            var requestedSeats = 1;
            var bookingRequestsCount = 5;
            var activeThreadsCount = 0;
            var maxSimultaneousThreadsCount = 0;
            var semaphoreThreadsCountSetting = 1;
            var lockObject = new Lock();
            var timeoutTask = Task.Delay(5000, CancellationToken.None);

            var tempProvider = _services.BuildServiceProvider();
            var eventService = tempProvider.GetRequiredService<IEventService>();
            var mockEventService = new Mock<IEventService>();
            mockEventService.SetReturnsDefault(eventService);

            mockEventService
                .Setup(mock => mock.GetByIdAsync(@event.Id, It.IsAny<CancellationToken>()))
                .Returns(async (Guid id, CancellationToken ct) =>
                {
                    lock (lockObject)
                    {
                        activeThreadsCount++;

                        if (activeThreadsCount > maxSimultaneousThreadsCount)
                        {
                            maxSimultaneousThreadsCount = activeThreadsCount;
                        }
                    }

                    await Task.Delay(50);

                    try
                    {
                        return await eventService.GetByIdAsync(id, ct);
                    }
                    finally
                    {
                        lock (lockObject)
                        {
                            activeThreadsCount--;
                        }
                    }
                });

            _services.Remove(_services.First(d => d.ServiceType == typeof(IEventService)));
            _services.AddScoped<IEventService>(_ => mockEventService.Object);
            var localProvider = _services.BuildServiceProvider();

            /*---ACT---*/
            var tasks = new List<Task<Booking>>();

            for (int i = 0; i < bookingRequestsCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                    {
                        using var scope = localProvider.CreateScope();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                        return await bookingService.CreateBookingAsync(@event.Id, user.Id, requestedSeats, CancellationToken.None);
                    }));
            }

            var creatingBookingTask = Task.WhenAll(tasks);

            var firstCompletedTask = await Task.WhenAny(creatingBookingTask, timeoutTask);

            /*---ASSERT---*/
            if (firstCompletedTask == timeoutTask)
            {
                Assert.Fail("Тест завис. Вероятнее всего, забыли освободить семафор через Release.");
            }

            Assert.Equal(semaphoreThreadsCountSetting, maxSimultaneousThreadsCount);
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

        private async Task<Event> AddEventToContextAsync(
            int number,
            DateTime startAt,
            DateTime endAt,
            int totalSeats)
        {
            var @event = new Event(
                new Guid($"E0000000-0000-0000-0000-00000000000{number}"),
                $"Event {number}",
                $"Description {number}",
                startAt,
                endAt,
                totalSeats);

            await _context.Events.AddAsync(@event);

            return @event;
        }

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

        private async Task<User> AddUserToContextAsync(
            int number,
            UserRole role = UserRole.User)
        {
            var user = new User(
                new Guid($"A0000000-0000-0000-0000-00000000000{number}"),
                $"User {number}",
                $"Hashed Password {number}",
                role);

            await _context.Users.AddAsync(user);

            return user;
        }
    }
}
