using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
using EventsRestApi.Models;
using EventsRestApi.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventsRestApi.Tests
{
    public class BookingServiceTests
    {
        private readonly BookingService _bookingService;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IEventService> _mockEventService;

        public BookingServiceTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockEventService = new Mock<IEventService>();

            _bookingService = new BookingService(
                _mockBookingRepository.Object,
                _mockEventService.Object,
                NullLogger<BookingService>.Instance);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithExistingId_ReturnBooking()
        {
            /*---ARRANGE---*/
            var bookingId = new Guid("00000000-0000-0000-0000-000000000001");

            var booking = new Booking(
                bookingId,
                new Guid("00000000-0000-0000-0000-000000000001"),
                BookingStatus.Pending,
                new DateTime(2026, 06, 01, 09, 01, 01)
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
        public async Task GetBookingByIdAsync_AfterChangingStatus_ReturnWithChangedStatus()
        {
            /*---ARRANGE---*/
            var bookingId = new Guid("00000000-0000-0000-0000-000000000001");

            var booking = new Booking(
                bookingId,
                new Guid("00000000-0000-0000-0000-000000000001"),
                BookingStatus.Confirmed,
                new DateTime(2026, 06, 01, 09, 01, 01),
                new DateTime(2026, 06, 01, 09, 01, 03)
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
        public async Task CreateBookingAsync_WithExistingEvent_ReturnCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("00000000-0000-0000-0000-000000000001");

            var addedBooking = new Booking(
                new Guid("00000000-0000-0000-0000-000000000001"),
                eventId,
                BookingStatus.Pending,
                new DateTime(2026, 06, 01, 09, 01, 01)
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
        public async Task CreateBookingAsync_ForEventThatHasAnotherBooking_ReturnNewCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("00000000-0000-0000-0000-000000000001");

            _mockEventService
                .Setup(mock => mock.IsEventStillValid(eventId))
                .Returns(true);

            _mockBookingRepository
                .Setup(mock => mock.Add(eventId))
                .Returns(() => new Booking(
                    Guid.NewGuid(),
                    eventId,
                    BookingStatus.Pending,
                    new DateTime(2026, 06, 01, 09, 01, 01)
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
        public async Task CreateBookingAsync_WithInabilityCreateBookingForEvent_ReturnNull()
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
        public async Task ProcessPendingBookingsAsync_BookingsLessThanBunch_CheckConfirmStatusForAll()
        {
            /*---ARRANGE---*/
            var processingBookingsCount = 5;

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("00000000-0000-0000-0000-000000000001"),
                    new Guid("00000000-0000-0000-0000-000000000001"),
                    BookingStatus.Pending,
                    new DateTime(2026, 06, 01, 09, 01, 01)),
                new Booking(
                    new Guid("00000000-0000-0000-0000-000000000002"),
                    new Guid("00000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    new DateTime(2026, 06, 02, 09, 02, 02))
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, processingBookingsCount))
                .Returns(pendingBookings);

            /*---ACT---*/
            var result = await _bookingService.ProcessPendingBookingsBunchAsync(processingBookingsCount, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[0].Status);
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[1].Status);
            Assert.Equal(pendingBookings.Count, result);

            _mockBookingRepository.Verify(mock => mock.Update(It.IsAny<Booking>()), Times.Exactly(pendingBookings.Count));
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_BookingsMoreThanBunch_CheckConfirmStatusForFirst()
        {
            /*---ARRANGE---*/
            var processingBookingsCount = 1;

            var pendingBookings = new List<Booking>()
            {
                new Booking(
                    new Guid("00000000-0000-0000-0000-000000000001"),
                    new Guid("00000000-0000-0000-0000-000000000001"),
                    BookingStatus.Pending,
                    new DateTime(2026, 06, 01, 09, 01, 01)),
                new Booking(
                    new Guid("00000000-0000-0000-0000-000000000002"),
                    new Guid("00000000-0000-0000-0000-000000000002"),
                    BookingStatus.Pending,
                    new DateTime(2026, 06, 02, 09, 02, 02))
            };

            _mockBookingRepository
                .Setup(mock => mock.GetByStatus(BookingStatus.Pending, processingBookingsCount))
                .Returns(pendingBookings.Take(processingBookingsCount).ToList());

            /*---ACT---*/
            var result = await _bookingService.ProcessPendingBookingsBunchAsync(processingBookingsCount, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[0].Status);
            Assert.Equal(BookingStatus.Pending, pendingBookings[1].Status);
            Assert.Equal(processingBookingsCount, result);

            _mockBookingRepository.Verify(mock => mock.Update(It.IsAny<Booking>()), Times.Exactly(processingBookingsCount));
        }
    }
}
