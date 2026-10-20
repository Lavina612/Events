using EventsRestApi.Presentation.Data.Repositories;
using EventsRestApi.Presentation.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace EventsRestApi.IntegrationsTests
{
    public class BookingRepositoryTests : BaseDbTest
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 01, 09, 01, 01, TimeSpan.Zero);
        private readonly FakeTimeProvider _fakeTimeProvider;

        public BookingRepositoryTests(DbFixture fixture) : base(fixture)
        {
            _fakeTimeProvider = new FakeTimeProvider();
            _fakeTimeProvider.SetUtcNow(fixedTime);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_TwoBookingsInDb_ReturnAllBookings()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                2);

            var booking1 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(1));

            var booking2 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000002"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(2));

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.Bookings.AddRangeAsync(booking1, booking2);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.GetAllAsync(CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(2, result.Count);
            result.Should().BeEquivalentTo([booking1, booking2], options => options.Excluding(x => x.Event));
        }

        [Fact]
        public async Task GetAllAsync_NoBookingsInDb_ReturnEmptyCollection()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.GetAllAsync(CancellationToken.None);

            /*---ASSERT---*/
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnBookingById()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                11);

            var booking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(1));

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.Bookings.AddAsync(booking);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.GetByIdAsync(booking.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(result);
            result.Should().BeEquivalentTo(booking, options => options.Excluding(x => x.Event));
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.GetByIdAsync(Guid.Empty, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Null(result);
        }

        #endregion

        #region GetIdsByStatusAsync Tests

        [Fact]
        public async Task GetIdsByStatusAsync_EverythingIsCorrect_ReturnIdsByStatus()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                5);

            var booking1 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                BookingStatus.Confirmed,
                1,
                fixedTime.UtcDateTime.AddHours(1));

            var booking2 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000002"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(2));

            var booking3 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000003"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(3));

            var booking4 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000004"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(4));

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.Bookings.AddRangeAsync(booking1, booking2, booking3, booking4);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.GetIdsByStatusAsync(BookingStatus.Pending, 2, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(2, result.Count);
            Assert.Equal(booking2.Id, result[0]);
            Assert.Equal(booking3.Id, result[1]);
        }

        [Fact]
        public async Task GetIdsByStatusAsync_NoBookingsInDb_ReturnEmptyCollection()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.GetIdsByStatusAsync(BookingStatus.Pending, 5, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Empty(result);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_EverythingIsCorrect_ReturnAddedBooking()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var requestedSeats = 1;

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                1);

            var expectedBooking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                BookingStatus.Pending,
                requestedSeats,
                fixedTime.UtcDateTime);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.AddAsync(@event.Id, requestedSeats, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var addedBooking = assertContext.Bookings.FirstOrDefault(x => x.Id == result.Id);

            Assert.NotNull(result);
            Assert.NotNull(addedBooking);
            Assert.Equivalent(result, addedBooking, true);
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(expectedBooking, options => options.Excluding(x => x.Id));
        }

        [Fact]
        public async Task AddAsync_WithNonExistentEventId_ThrowDbUpdateException()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT & ASSERT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
                await bookingRepository.AddAsync(Guid.Empty, 1, CancellationToken.None));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnFalse()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                1);

            var booking = new Booking(
                Guid.Empty,
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(1));

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.UpdateAsync(booking, CancellationToken.None);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_CorrectBooking_ReturnTrueAndBookingWasUpdated()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                1);

            var booking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(1));

            var newBooking = new Booking(
                booking.Id,
                booking.EventId,
                BookingStatus.Confirmed,
                booking.BookedSeats + 1,
                fixedTime.UtcDateTime.AddHours(2));

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.Bookings.AddAsync(booking);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.UpdateAsync(newBooking, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var updatedBooking = assertContext.Bookings.FirstOrDefault(x => x.Id == booking.Id);

            Assert.True(result);
            Assert.NotNull(updatedBooking);
            Assert.Equal(newBooking.Status, updatedBooking.Status);
            Assert.Equal(newBooking.ProcessedAt, updatedBooking.ProcessedAt);

            updatedBooking.Should().BeEquivalentTo(booking, options => options
                .Excluding(x => x.Status)
                .Excluding(x => x.ProcessedAt)
                .Excluding(x => x.Event));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnFalse()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.DeleteAsync(Guid.Empty, CancellationToken.None);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_CorrectId_ReturnTrueAndBookingWasDeleted()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                1);

            var booking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime.AddHours(1));

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.Bookings.AddAsync(booking);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var bookingRepository = new BookingRepository(actContext, _fakeTimeProvider);
            var result = await bookingRepository.DeleteAsync(booking.Id, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var deletedBooking = assertContext.Bookings.FirstOrDefault(x => x.Id == booking.Id);

            Assert.True(result);
            Assert.Null(deletedBooking);
        }

        #endregion

        #region SaveContextAsync Tests

        [Fact]
        public async Task SaveContextAsync_AddBookingToContext_BookingWasAddedToDb()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                1);

            var booking = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime);

            await using var context = CreateContext();
            await context.Events.AddAsync(@event);
            await context.Bookings.AddAsync(booking);

            /*---ACT---*/
            var bookingRepository = new BookingRepository(context, _fakeTimeProvider);
            await bookingRepository.SaveContextAsync(CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var foundBooking = assertContext.Bookings.FirstOrDefault(x => x.Id == booking.Id);

            Assert.NotNull(foundBooking);

            foundBooking.Should().BeEquivalentTo(booking, options => options.Excluding(x => x.Event));
        }

        #endregion
    }
}