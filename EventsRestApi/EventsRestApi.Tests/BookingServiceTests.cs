using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
using EventsRestApi.Models;
using EventsRestApi.Services;
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

            _bookingService = new BookingService(_mockBookingRepository.Object, _mockEventService.Object);
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

            _mockBookingRepository.Setup(mock => mock.GetById(bookingId)).Returns(booking);

            /*---ACT---*/
            var result = await _bookingService.GetBookingByIdAsync(bookingId, It.IsAny<CancellationToken>());

            /*---ASSERT---*/
            Assert.Equivalent(expectedBookingDto, result, true);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            var bookingId = Guid.Empty;

            _mockBookingRepository.Setup(mock => mock.GetById(bookingId)).Returns((Booking?)null);

            /*---ACT---*/
            var result = await _bookingService.GetBookingByIdAsync(bookingId, It.IsAny<CancellationToken>());

            /*---ASSERT---*/
            Assert.Null(result);
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

            _mockEventService.Setup(mock => mock.CanCreateBooking(eventId)).Returns(true);
            _mockBookingRepository.Setup(mock => mock.Add(eventId)).Returns(addedBooking);

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(eventId, It.IsAny<CancellationToken>());

            /*---ASSERT---*/
            Assert.Equivalent(expectedAddedBookingDto, result, true);
        }

        [Fact]
        public async Task CreateBookingAsync_ForEventThatHasAnotherBooking_ReturnNewCreatedBooking()
        {
            /*---ARRANGE---*/
            var eventId = new Guid("00000000-0000-0000-0000-000000000001");

            _mockEventService.Setup(mock => mock.CanCreateBooking(eventId)).Returns(true);
            _mockBookingRepository.Setup(mock => mock.Add(eventId)).Returns(() => new Booking(
                Guid.NewGuid(),
                eventId,
                BookingStatus.Pending,
                new DateTime(2026, 06, 01, 09, 01, 01)
                ));

            /*---ACT---*/
            var firstResult = await _bookingService.CreateBookingAsync(eventId, It.IsAny<CancellationToken>());
            var secondResult = await _bookingService.CreateBookingAsync(eventId, It.IsAny<CancellationToken>());

            /*---ASSERT---*/
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.NotEqual(firstResult.Id, secondResult.Id);

            _mockEventService.Verify(mock => mock.CanCreateBooking(eventId), Times.Exactly(2));
            _mockBookingRepository.Verify(mock => mock.Add(eventId), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateBookingAsync_WithInabilityCreateBookingForEvent_ReturnNull()
        {
            /*---ARRANGE---*/
            var eventId = Guid.Empty;

            _mockEventService.Setup(mock => mock.CanCreateBooking(eventId)).Returns(false);

            /*---ACT---*/
            var result = await _bookingService.CreateBookingAsync(eventId, It.IsAny<CancellationToken>());

            /*---ASSERT---*/
            Assert.Null(result);

            _mockBookingRepository.Verify(mock => mock.Add(eventId), Times.Never());
        }

        [Fact]
        public async Task ProcessPendingBookingsAsync_AfterFiveSeconds_CheckConfirmStatus()
        {
            /*---ARRANGE---*/
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

            _mockBookingRepository.Setup(mock => mock.GetByStatus(BookingStatus.Pending)).Returns(pendingBookings);

            /*---ACT---*/
            await _bookingService.ProcessPendingBookingsAsync(It.IsAny<CancellationToken>());

            await Task.Delay(5000);

            /*---ASSERT---*/
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[0].Status);
            Assert.Equal(BookingStatus.Confirmed, pendingBookings[1].Status);
        }
    }
}
