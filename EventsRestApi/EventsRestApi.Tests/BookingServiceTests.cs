using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
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

            var booking = new Booking(
                bookingId,
                new Guid("E0000000-0000-0000-0000-000000000001"),
                BookingStatus.Pending,
                fixedTime.UtcDateTime
                );

            var expectedBookingDto = Mapper.MapToBookingResponseDto(booking);

            _mockBookingRepository
                .Setup(mock => mock.GetById(bookingId))
                .Returns(booking);

            /*---ACT---*/
            var result = await _bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expectedBookingDto, result, true);
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
        public async Task CreateBookingAsync_WithExistingValidEvent_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            var addedBooking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                eventId,
                BookingStatus.Pending,
                createdAt
                );

            var expectedAddedBookingDto = Mapper.MapToBookingResponseDto(addedBooking);

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(eventId))
                .Returns(true);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId))
                .Returns(addedBooking);

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expectedAddedBookingDto, result, true);
        }

        [Fact]
        public async Task CreateBookingAsync_WithEventThatHasAnotherBooking_ReturnNewCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("E0000000-0000-0000-0000-000000000001");

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var createdAt = fixedTime.UtcDateTime;

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(eventId))
                .Returns(true);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId))
                .Returns(() => new Booking(
                    Guid.NewGuid(),
                    eventId,
                    BookingStatus.Pending,
                    createdAt
                    ));

            /*---ACT---*/
            var firstResult = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);
            var secondResult = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.NotEqual(firstResult.Id, secondResult.Id);

            _mockEventService.Verify(mock => mock.IsEventStillValid(eventId), Times.Exactly(2));
            _mockBookingRepository.Verify(mock => mock.Add(eventId), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateBookingAsync_WithNotValidEvent_ReturnNull()
        {
            /*---ARRANGE---*/
            var eventId = Guid.Empty;

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(eventId))
                .Returns(false);

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Null(result);

            _mockBookingRepository.Verify(mock => mock.Add(eventId), Times.Never());
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_EventsAreStillValid_CheckConfirmedStatus()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var firstBookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;
            var secondBookingCreatedAt = fixedTime.AddHours(-1).AddMinutes(1).AddSeconds(1).UtcDateTime;

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    new Guid("E0000000-0000-0000-0000-000000000001"),
                    BookingStatus.Pending,
                    firstBookingCreatedAt),
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000002"),
                    new Guid("E0000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    secondBookingCreatedAt)
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(It.IsAny<Guid>()))
                .Returns(true);

            /*---ACT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(pendingBookings.Count, CancellationToken.None);

            while (!bookingServiceTask.IsCompleted)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
            }

            var result = await bookingServiceTask;

            /*---ASSERT---*/
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[0].Status);
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[1].Status);
            Assert.NotNull(pendingBookings[0].ProcessedAt);
            Assert.NotNull(pendingBookings[1].ProcessedAt);
            Assert.True(pendingBookings[0].ProcessedAt > pendingBookings[0].CreatedAt);
            Assert.True(pendingBookings[1].ProcessedAt > pendingBookings[1].CreatedAt);
            Assert.Equal(pendingBookings.Count, result);

            _mockBookingRepository
                .Verify(mock => mock.Update(It.Is<Booking>(x => x.Status == BookingStatus.Confirmed && x.ProcessedAt != null)),
                    Times.Exactly(pendingBookings.Count));
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_EventsAreNoLongerValid_CheckRejectedStatus()
        {
            /*---ARRANGE---*/
            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var firstBookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;
            var secondBookingCreatedAt = fixedTime.AddHours(-1).AddMinutes(1).AddSeconds(1).UtcDateTime;

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    new Guid("E0000000-0000-0000-0000-000000000001"),
                    BookingStatus.Pending,
                    firstBookingCreatedAt),
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000002"),
                    new Guid("E0000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    secondBookingCreatedAt)
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(It.IsAny<Guid>()))
                .Returns(false);

            /*---ACT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(pendingBookings.Count, CancellationToken.None);

            while (!bookingServiceTask.IsCompleted)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
            }

            var result = await bookingServiceTask;

            /*---ASSERT---*/
            Assert.Equal(BookingStatus.Rejected, pendingBookings[0].Status);
            Assert.Equal(BookingStatus.Rejected, pendingBookings[1].Status);
            Assert.NotNull(pendingBookings[0].ProcessedAt);
            Assert.NotNull(pendingBookings[1].ProcessedAt);
            Assert.True(pendingBookings[0].ProcessedAt > pendingBookings[0].CreatedAt);
            Assert.True(pendingBookings[1].ProcessedAt > pendingBookings[1].CreatedAt);
            Assert.Equal(pendingBookings.Count, result);

            _mockBookingRepository
                .Verify(mock => mock.Update(It.Is<Booking>(x => x.Status == BookingStatus.Rejected && x.ProcessedAt != null)),
                    Times.Exactly(pendingBookings.Count));
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
        public async Task ProcessPendingBookingsBunchAsync_CancelAfterProcessFirstBooking_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var firstBookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;
            var secondBookingCreatedAt = fixedTime.AddHours(-1).AddMinutes(1).AddSeconds(1).UtcDateTime;

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    new Guid("E0000000-0000-0000-0000-000000000001"),
                    BookingStatus.Pending,
                    firstBookingCreatedAt),
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000002"),
                    new Guid("E0000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    secondBookingCreatedAt)
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(It.IsAny<Guid>()))
                .Returns(true);

            _mockBookingRepository
                .Setup(mock => mock.Update(pendingBookings[0]))
                .Callback<Booking>(x => cts.Cancel());

            /*---ACT---*/
            var bookingServiceTask = _bookingService.ProcessPendingBookingsBunchAsync(pendingBookings.Count, cts.Token);

            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));

            /*---ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => bookingServiceTask);

            Assert.Equal(cts.Token, exception.CancellationToken);
            Assert.Contains("ThrowIfCancellationRequested", exception.StackTrace);
        }

        [Fact]
        public async Task ProcessPendingBookingsBunchAsync_CancelTaskWhenTaskDelay_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();

            //Дата createdAt высчитывается относительно fixedTime: 2026.06.01 09:01:01
            var firstBookingCreatedAt = fixedTime.AddHours(-1).UtcDateTime;
            var secondBookingCreatedAt = fixedTime.AddHours(-1).AddMinutes(1).AddSeconds(1).UtcDateTime;

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000001"),
                    new Guid("E0000000-0000-0000-0000-000000000001"),
                    BookingStatus.Pending,
                    firstBookingCreatedAt),
                new Booking(
                    new Guid("B0000000-0000-0000-0000-000000000002"),
                    new Guid("E0000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    secondBookingCreatedAt)
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, pendingBookings.Count))
                .Returns(pendingBookings);

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(It.IsAny<Guid>()))
                .Returns(true);

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
