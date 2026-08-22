using EventsRestApi.Exceptions;
using EventsRestApi.Interfaces;
using EventsRestApi.Models;
using EventsRestApi.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace EventsRestApi.Tests
{
    public class BookingServiceTests
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 01, 09, 01, 01, TimeSpan.Zero);
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly BookingService _bookingService;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IEventService> _mockEventService;

        public BookingServiceTests()
        {
            _fakeTimeProvider = new FakeTimeProvider();
            _fakeTimeProvider.SetUtcNow(fixedTime);

            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockEventService = new Mock<IEventService>();

            _bookingService = new BookingService(
                _mockBookingRepository.Object,
                _mockEventService.Object,
                _fakeTimeProvider,
                NullLogger<BookingService>.Instance);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithExistingId_ReturnBooking()
        {
            /*---ARRANGE---*/
            var bookingId = new Guid("B0000000-0000-0000-0000-000000000001");

            var expectedBooking = new Booking(
                bookingId,
                new Guid("E0000000-0000-0000-0000-000000000001"),
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime
                );

            _mockBookingRepository
                .Setup(mock => mock.GetById(bookingId))
                .Returns(expectedBooking);

            /*---ACT---*/
            var result = await _bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expectedBooking, result, true);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            var bookingId = Guid.Empty;

            _mockBookingRepository
                .Setup(mock => mock.GetById(bookingId))
                .Returns((Booking?)null);

            /*---ACT---*/
            var result = await _bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateBookingAsync_WithExistingValidEventWithEnoughAvailableSeats_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                createdAt.AddDays(-1),
                createdAt.AddDays(1),
                1);

            var expectedCreatedBooking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                eventId,
                BookingStatus.Pending,
                1,
                createdAt
                );

            var availableSeatsAfterBooking = foundEvent.AvailableSeats - expectedCreatedBooking.BookedSeats;

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId, expectedCreatedBooking.BookedSeats))
                .Returns(expectedCreatedBooking);

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(eventId, expectedCreatedBooking.BookedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expectedCreatedBooking, result, true);
            Assert.Equal(availableSeatsAfterBooking, foundEvent.AvailableSeats);

            _mockEventService.Verify(mock => mock.Update(foundEvent),
                Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_ForEventThatHasAnotherBooking_ReturnNewCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");
            var requestedSeats = 1;
            var successfulBookingsCount = 2;

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                createdAt.AddDays(-1),
                createdAt.AddDays(1),
                2);

            var availableSeatsAfterBooking = foundEvent.AvailableSeats - requestedSeats * successfulBookingsCount;

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId, requestedSeats))
                .Returns(() => new Booking(
                    Guid.NewGuid(),
                    eventId,
                    BookingStatus.Pending,
                    requestedSeats,
                    createdAt
                    ));

            /*---ACT---*/
            var firstResult = await _bookingService.CreateBookingAsync(eventId, requestedSeats, CancellationToken.None);
            var secondResult = await _bookingService.CreateBookingAsync(eventId, requestedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.NotEqual(firstResult.Id, secondResult.Id);
            Assert.Equal(availableSeatsAfterBooking, foundEvent.AvailableSeats);

            _mockBookingRepository.Verify(mock => mock.Add(eventId, requestedSeats),
                Times.Exactly(successfulBookingsCount));

            _mockEventService.Verify(mock => mock.Update(It.IsAny<Event>()),
                Times.Exactly(successfulBookingsCount));
        }

        [Fact]
        public async Task CreateBookingAsync_BookSeatAfterRelease_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                createdAt.AddDays(-1),
                createdAt.AddDays(1),
                1);

            foundEvent.TryReserveSeats(1);

            var expectedCreatedBooking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                eventId,
                BookingStatus.Pending,
                1,
                createdAt
                );

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId, expectedCreatedBooking.BookedSeats))
                .Returns(expectedCreatedBooking);

            /*---ACT---*/
            foundEvent.ReleaseSeats(1);
            var availableSeatsAfterBooking = foundEvent.AvailableSeats - expectedCreatedBooking.BookedSeats;
            var result = await _bookingService.CreateBookingAsync(eventId, expectedCreatedBooking.BookedSeats, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expectedCreatedBooking, result, true);
            Assert.Equal(availableSeatsAfterBooking, foundEvent.AvailableSeats);

            _mockEventService.Verify(mock => mock.Update(foundEvent), 
                Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_TryCreateAfterNoMoreAvailableSeats_ThrowNoAvailableSeatsException()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");
            var requestedSeats = 1;
            var successfulBookingsCount = 1;

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                createdAt.AddDays(-1),
                createdAt.AddDays(1),
                1);

            var expectedCreatedBooking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                eventId,
                BookingStatus.Pending,
                requestedSeats,
                createdAt
                );

            var availableSeatsAfterBooking = foundEvent.AvailableSeats - requestedSeats * successfulBookingsCount;

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId, requestedSeats))
                .Returns(expectedCreatedBooking);

            /*---ACT & ASSERT---*/
            var firstResult = await _bookingService.CreateBookingAsync(eventId, requestedSeats, CancellationToken.None);

            var secondResultException = await Assert.ThrowsAnyAsync<NoAvailableSeatsException>(() =>
                _bookingService.CreateBookingAsync(eventId, requestedSeats, CancellationToken.None));

            Assert.Equivalent(expectedCreatedBooking, firstResult, true);
            Assert.Equal(availableSeatsAfterBooking, foundEvent.AvailableSeats);

            _mockBookingRepository.Verify(mock => mock.Add(eventId, requestedSeats),
                Times.Exactly(successfulBookingsCount));

            _mockEventService.Verify(mock => mock.Update(It.IsAny<Event>()),
                Times.Exactly(successfulBookingsCount));
        }

        [Fact]
        public async Task CreateBookingAsync_WithNonExistentEvent_ThrowNotFoundEventException()
        {
            /*---ARRANGE---*/
            var eventId = Guid.Empty;

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns((Event?)null);

            /*---ACT & ASSERT---*/
            await Assert.ThrowsAnyAsync<NotFoundEventException>(() =>
                _bookingService.CreateBookingAsync(eventId, It.IsAny<int>(), CancellationToken.None));

            _mockBookingRepository.Verify(mock => mock.Add(eventId, It.IsAny<int>()),
                Times.Never);

            _mockEventService.Verify(mock => mock.Update(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_WithFinishedEvent_ThrowFinishedEventException()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");

            //Дата startAt высчитывается относительно fixedTime: 2026.06.06 09:06:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(1).UtcDateTime;
            var endAt = startAt.AddDays(1);

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                startAt,
                endAt,
                1);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            /*---ACT & ASSERT---*/
            await Assert.ThrowsAnyAsync<FinishedEventException>(() =>
                _bookingService.CreateBookingAsync(eventId, 1, CancellationToken.None));

            _mockBookingRepository.Verify(mock => mock.Add(eventId, It.IsAny<int>()),
                Times.Never);

            _mockEventService.Verify(mock => mock.Update(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_WithRequestedSeatsMoreThanAvailable_ThrowNoAvailableSeatsException()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");

            //Дата startAt высчитывается относительно fixedTime: 2026.06.06 09:06:00
            var startAt = fixedTime.AddDays(1).AddMinutes(1).UtcDateTime;
            var endAt = startAt.AddDays(5);

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                startAt,
                endAt,
                1);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<NoAvailableSeatsException>(() =>
                _bookingService.CreateBookingAsync(eventId, 5, CancellationToken.None));

            _mockBookingRepository.Verify(mock => mock.Add(eventId, It.IsAny<int>()),
                Times.Never);

            _mockEventService.Verify(mock => mock.Update(It.IsAny<Event>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_ManyConcurrentAttemptToCreateBooking_NoOverbookingForEvent()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");
            var requestedSeats = 1;
            var eventTotalSeats = 5;
            var bookingRequestsCount = 20;
            var uniqueCreatedBookingsId = new HashSet<Guid>();

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                createdAt.AddDays(-1),
                createdAt.AddDays(1),
                eventTotalSeats);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId, requestedSeats))
                .Returns(() => new Booking(
                    Guid.NewGuid(),
                    eventId,
                    BookingStatus.Pending,
                    requestedSeats,
                    createdAt
                    ));

            /*---ACT---*/
            var tasks = new List<Task<Booking>>();

            for (int i = 0; i < bookingRequestsCount; i++)
            {
                tasks.Add(_bookingService.CreateBookingAsync(eventId, requestedSeats, CancellationToken.None));
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

            Assert.Equal(0, foundEvent.AvailableSeats);

            _mockBookingRepository.Verify(mock => mock.Add(eventId, requestedSeats),
                Times.Exactly(eventTotalSeats));

            _mockEventService.Verify(mock => mock.Update(foundEvent),
                Times.Exactly(eventTotalSeats));
        }

        [Fact]
        public async Task CreateBookingAsync_CheckParallelTasksCount_SemaphoreIsSetCorrectly()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");
            var requestedSeats = 1;
            var bookingRequestsCount = 5;
            var activeThreadsCount = 0;
            var maxSimultaneousThreadsCount = 0;
            var semaphoreThreadsCountSetting = 1;
            var lockObject = new Lock();

            var timeoutTask = Task.Delay(5000);

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                createdAt.AddDays(-1),
                createdAt.AddDays(1),
                5);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(() =>
                {
                    lock (lockObject)
                    {
                        activeThreadsCount++;

                        if (activeThreadsCount > maxSimultaneousThreadsCount)
                        {
                            maxSimultaneousThreadsCount = activeThreadsCount;
                        }
                    }

                    Thread.Sleep(50);

                    lock (lockObject)
                    {
                        activeThreadsCount--;
                    }

                    return foundEvent;
                });

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId, requestedSeats))
                .Returns(() => new Booking(
                    Guid.NewGuid(),
                    eventId,
                    BookingStatus.Pending,
                    requestedSeats,
                    createdAt
                    ));

            /*---ACT---*/
            var tasks = new List<Task<Booking>>();

            for (int i = 0; i < bookingRequestsCount; i++)
            {
                tasks.Add(Task.Run(() => _bookingService.CreateBookingAsync(eventId, requestedSeats, CancellationToken.None)));
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
        public async Task ProcessPendingBookingsBunchAsync_BookingsAndEventsAreCorrect_CheckConfirmedStatus()
        {
            /*---ARRANGE---*/
            var bunchCount = 1;

            //Дата bookingCreatedAt высчитывается относительно fixedTime: 2026.06.01 08:01:01
            var bookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;

            var firstEvent = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                bookingCreatedAt.AddDays(-1),
                bookingCreatedAt.AddDays(1),
                1);

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    firstEvent.Id,
                    BookingStatus.Pending,
                    1,
                    bookingCreatedAt),
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000002"),
                    new Guid("E0000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    1,
                    bookingCreatedAt)
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, bunchCount))
                .Returns([pendingBookings[0]]);

            _mockEventService
                .Setup(mock => mock.GetById(firstEvent.Id))
                .Returns(firstEvent);

            /*---ACT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(bunchCount, CancellationToken.None);

            while (!bookingServiceTask.IsCompleted)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
            }

            var result = await bookingServiceTask;

            /*---ASSERT---*/
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[0].Status);
            Assert.NotNull(pendingBookings[0].ProcessedAt);
            Assert.True(pendingBookings[0].ProcessedAt > pendingBookings[0].CreatedAt);
            Assert.Equal(bunchCount, result);

            _mockBookingRepository
                .Verify(mock => mock.Update(It.Is<Booking>(x => x.Status == BookingStatus.Confirmed && x.ProcessedAt != null)),
                    Times.Exactly(bunchCount));
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_EventDoesNotExistMore_CheckRejectedStatus()
        {
            /*---ARRANGE---*/
            var eventId = Guid.Empty;

            //Дата bookingCreatedAt высчитывается относительно fixedTime: 2026.06.01 08:01:01
            var bookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    eventId,
                    BookingStatus.Pending,
                    1,
                    bookingCreatedAt)
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns((Event?)null);

            /*---ACT & ASSERT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(pendingBookings.Count, CancellationToken.None);

            while (!bookingServiceTask.IsCompleted)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
            }

            var result = await bookingServiceTask;

            Assert.Equal(BookingStatus.Rejected, pendingBookings[0].Status);
            Assert.NotNull(pendingBookings[0].ProcessedAt);
            Assert.True(pendingBookings[0].ProcessedAt > pendingBookings[0].CreatedAt);

            _mockBookingRepository.Verify(mock => mock.Update(It.Is<Booking>(x => x.Status == BookingStatus.Rejected && x.ProcessedAt != null)),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_EventWasFinished_CheckRejectedStatus()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");
            var eventAvailableSeatsBeforeBooking = 1;

            //Дата bookingCreatedAt высчитывается относительно fixedTime: 2026.06.01 08:01:01
            var bookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                bookingCreatedAt.AddDays(-2),
                bookingCreatedAt.AddDays(-1),
                eventAvailableSeatsBeforeBooking);

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    eventId,
                    BookingStatus.Pending,
                    1,
                    bookingCreatedAt)
            };

            foundEvent.TryReserveSeats(pendingBookings[0].BookedSeats);

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            /*---ACT & ASSERT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(pendingBookings.Count, CancellationToken.None);

            while (!bookingServiceTask.IsCompleted)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
            }

            var result = await bookingServiceTask;

            Assert.Equal(BookingStatus.Rejected, pendingBookings[0].Status);
            Assert.NotNull(pendingBookings[0].ProcessedAt);
            Assert.True(pendingBookings[0].ProcessedAt > pendingBookings[0].CreatedAt);
            Assert.Equal(eventAvailableSeatsBeforeBooking, foundEvent.AvailableSeats);

            _mockEventService.Verify(mock => mock.Update(It.Is<Event>(x => x.AvailableSeats == eventAvailableSeatsBeforeBooking)),
                Times.Once);

            _mockBookingRepository.Verify(mock => mock.Update(It.Is<Booking>(x => x.Status == BookingStatus.Rejected && x.ProcessedAt != null)),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_SomethingWasWrong_CatchAnyExceptionWithoutCrash()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");
            var eventAvailableSeatsBeforeBooking = 1;

            //Дата bookingCreatedAt высчитывается относительно fixedTime: 2026.06.01 08:01:01
            var bookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;

            var foundEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                bookingCreatedAt.AddDays(-1),
                bookingCreatedAt.AddDays(1),
                eventAvailableSeatsBeforeBooking);

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    eventId,
                    BookingStatus.Pending,
                    1,
                    bookingCreatedAt)
            };

            foundEvent.TryReserveSeats(pendingBookings[0].BookedSeats);

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(foundEvent);

            _mockBookingRepository
                .SetupSequence(mock => mock.Update(pendingBookings[0]))
                .Throws(new InvalidOperationException("Ошибка с сохранением данных."))
                .Returns(true);

            /*---ACT & ASSERT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(pendingBookings.Count, CancellationToken.None);

            while (!bookingServiceTask.IsCompleted)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
            }

            var result = await bookingServiceTask;

            Assert.Equal(BookingStatus.Rejected, pendingBookings[0].Status);
            Assert.NotNull(pendingBookings[0].ProcessedAt);
            Assert.True(pendingBookings[0].ProcessedAt > pendingBookings[0].CreatedAt);
            Assert.Equal(eventAvailableSeatsBeforeBooking, foundEvent.AvailableSeats);

            _mockEventService.Verify(mock => mock.Update(It.Is<Event>(x => x.AvailableSeats == eventAvailableSeatsBeforeBooking)),
                Times.Once);

            _mockBookingRepository.Verify(mock => mock.Update(It.IsAny<Booking>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _bookingService.ProcessPendingBookingsBunchAsync(It.IsAny<int>(), cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);

            _mockBookingRepository.Verify(mock => mock.GetByStatus(It.IsAny<BookingStatus>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_CancelTaskWhenTaskDelay_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");

            //Дата bookingCreatedAt высчитывается относительно fixedTime: 2026.06.01 08:01:01
            var bookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;

            var firstEvent = new Event(
                eventId,
                "Event 1",
                "Description 1",
                bookingCreatedAt.AddDays(-2),
                bookingCreatedAt.AddDays(1),
                1);

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    eventId,
                    BookingStatus.Pending,
                    1,
                    bookingCreatedAt),
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000002"),
                    new Guid("E0000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    1,
                    bookingCreatedAt)
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.GetById(eventId))
                .Returns(firstEvent);

            /*---ACT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(pendingBookings.Count, cts.Token);

            cts.Cancel();

            /*---ASSERT---*/
            //Если забыли передать токен отмены в Task.Delay, то пусть свалится по тайм-ауту
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await bookingServiceTask.WaitAsync(TimeSpan.FromMilliseconds(500)));

            Assert.Equal(cts.Token, exception.CancellationToken);

            _mockBookingRepository.Verify(mock => mock.Update(pendingBookings[0]),
                Times.Never);
        }
    }
}
