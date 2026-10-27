using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.EventService.Infrastructure.Data.Repositories;
using EventsRestApi.Services.SharedContracts.Models;
using FluentAssertions;

namespace EventsRestApi.Services.EventService.IntegrationTests
{
    public class EventRepositoryTests : BaseDbTest
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 05, 09, 05, 05, TimeSpan.Zero);
        private readonly List<Event> allEvents = [];

        public EventRepositoryTests(DbFixture fixture) : base(fixture)
        {
            FillAllEventsField();
        }

        #region GetPageAsync Tests

        [Fact]
        public async Task GetPageAsync_NoParamsWithBigPageSize_ReturnAllEvents()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();
            await AddAllEventsToDb();

            var page = 1;
            var pageSize = 100;
            List<Event> expectedEvents = allEvents.OrderBy(x => x.StartAt).ToList();

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithTitleParam_ReturnFilteredByTitleIgnoreCase()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();
            await AddAllEventsToDb();

            var titleParam = allEvents[0].Title.Substring(2).ToUpper();
            var page = 1;
            var pageSize = 100;
            List<Event> expectedEvents = [allEvents[0]];

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetPageAsync(titleParam, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithStartAtAndEndAtParams_ReturnFilteredBetweenDates()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();
            await AddAllEventsToDb();

            var page = 1;
            var pageSize = 100;

            //Дата startAt высчитывается относительно fixedTime: 2026.06.03 09:03:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(-2).AddSeconds(-5).UtcDateTime;
            var endAt = startAt.AddDays(6).AddMinutes(4).AddSeconds(4);

            List<Event> expectedEvents = allEvents[2..4].OrderBy(x => x.StartAt).ToList();

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetPageAsync(null, startAt, endAt, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithTitleAndStartAtAndEndAtParams_ReturnFilteredByTitleAndBetweenDates()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();
            await AddAllEventsToDb();

            var page = 1;
            var pageSize = 100;

            //Дата startAt высчитывается относительно fixedTime: 2026.06.03 09:03:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(-2).AddSeconds(-5).UtcDateTime;
            var endAt = startAt.AddDays(6).AddMinutes(4).AddSeconds(4);

            List<Event> expectedEvents = [allEvents[2]];

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetPageAsync("3", startAt, endAt, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithPageAndPageSize_ReturnEventsForPage()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();
            await AddAllEventsToDb();

            var page = 2;
            var pageSize = 2;
            List<Event> expectedEvents = allEvents.OrderBy(x => x.StartAt).Skip(2).Take(2).ToList();

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                allEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_NoEvents_ReturnEmptyPage()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var page = 1;
            var pageSize = 10;
            List<Event> expectedEvents = [];

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithPageMoreThanTotalPages_ReturnEmptyPage()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();
            await AddAllEventsToDb();

            var page = 10;
            var pageSize = 10;
            List<Event> expectedEvents = [];

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                allEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();
            await AddAllEventsToDb();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await eventRepository.GetPageAsync(string.Empty, default(DateTime), default(DateTime), 1, 100, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnEventById()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddRangeAsync(@event);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetByIdAsync(@event.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(result);
            Assert.Equivalent(@event, result, true);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetByIdAsync(Guid.Empty, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Null(result);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithEmptyEventId_ReturnAddedEventWithGeneratedId()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                Guid.Empty,
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.AddAsync(@event, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var addedEvent = assertContext.Events.FirstOrDefault(x => x.Id == result.Id);

            Assert.NotNull(result);
            Assert.NotNull(addedEvent);
            Assert.Equivalent(result, addedEvent, true);
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(@event, options => options.Excluding(x => x.Id));
        }

        [Fact]
        public async Task AddAsync_WithFilledEventId_ReturnAddedEventWithSameId()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.AddAsync(@event, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var addedEvent = assertContext.Events.FirstOrDefault(x => x.Id == result.Id);

            Assert.NotNull(result);
            Assert.NotNull(addedEvent);
            Assert.Equivalent(result, addedEvent, true);
            Assert.Equivalent(@event, result, true);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnFalse()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                Guid.Empty,
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.UpdateAsync(@event, CancellationToken.None);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_CorrectEvent_ReturnTrueAndEventWasUpdated()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.SaveChangesAsync();

            var updatingEvent = new Event(
                @event.Id,
                @event.Title + " New",
                @event.Description + " New",
                @event.StartAt.AddDays(-1),
                @event.EndAt.AddDays(1),
                @event.TotalSeats + 1);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.UpdateAsync(updatingEvent, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var updatedEvent = assertContext.Events.FirstOrDefault(x => x.Id == @event.Id);

            Assert.True(result);
            Assert.NotNull(updatedEvent);
            Assert.Equal(@event.TotalSeats, updatedEvent.TotalSeats);
            Assert.Equal(@event.AvailableSeats, updatedEvent.AvailableSeats);

            updatedEvent.Should().BeEquivalentTo(updatingEvent, options => options
                .Excluding(x => x.TotalSeats)
                .Excluding(x => x.AvailableSeats));
        }

        [Theory]
        // Представим временнную прямую с двумя контрольными точками: ----[1]-----[2]---->
        // Исходные даты сейчас совпадают с контрольными точками, т.е. StartAt = 1 = 01.06.26, EndAt = 2 = 06.06.26
        // В параметрах будем передавать смещение для StartAt и EndAt относительно исходных дат
        [InlineData(-10, -10)] // -S-E-[1]------[2]------>
        [InlineData(-1, -1)]   // ---S-[1]----E-[2]------>
        [InlineData(-1, 1)]    // ---S-[1]------[2]-E---->
        [InlineData(0, -1)]    // -----[1]=S--E-[2]------>
        [InlineData(0, 1)]     // -----[1]=S----[2]-E---->
        [InlineData(1, -1)]    // -----[1]-S--E-[2]------>
        [InlineData(1, 0)]     // -----[1]-S----[2]=E---->
        [InlineData(1, 1)]     // -----[1]-S----[2]-E---->
        [InlineData(10, 10)]   // -----[1]------[2]--S-E->
        public async Task UpdateAsync_CheckDifferentDatesShift_ReturnTrueWithNoExceptions(int startOffsetDays, int endOffsetDays)
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.SaveChangesAsync();

            var updatingEvent = new Event(
                @event.Id,
                @event.Title,
                @event.Description,
                @event.StartAt.AddDays(startOffsetDays),
                @event.EndAt.AddDays(endOffsetDays),
                @event.TotalSeats);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.UpdateAsync(updatingEvent, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var updatedEvent = assertContext.Events.FirstOrDefault(x => x.Id == @event.Id);

            Assert.True(result);
            Assert.NotNull(updatedEvent);
            Assert.Equal(updatingEvent.StartAt, updatedEvent.StartAt);
            Assert.Equal(updatingEvent.EndAt, updatedEvent.EndAt);
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
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.DeleteAsync(Guid.Empty, CancellationToken.None);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_CorrectId_ReturnTrueAndEventWasDeleted()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.DeleteAsync(@event.Id, CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var deletedEvent = assertContext.Events.FirstOrDefault(x => x.Id == @event.Id);

            Assert.True(result);
            Assert.Null(deletedEvent);
        }

        #endregion

        #region SaveContextAsync Tests

        [Fact]
        public async Task SaveContextAsync_AddEventToContext_EventWasAddedToDb()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            await using var context = CreateContext();
            await context.Events.AddAsync(@event);

            /*---ACT---*/
            var eventRepository = new EventRepository(context);
            await eventRepository.SaveContextAsync(CancellationToken.None);

            /*---ASSERT---*/
            await using var assertContext = CreateContext();
            var foundEvent = assertContext.Events.FirstOrDefault(x => x.Id == @event.Id);

            Assert.NotNull(foundEvent);
            Assert.Equivalent(@event, foundEvent, true);
        }

        #endregion

        private void FillAllEventsField()
        {
            var title = "Event";
            var description = "Description";
            var eventCount = 5;

            for (int i = 1; i <= eventCount; i++)
            {
                //Дата startAt высчитывается относительно fixedTime: 2026.06.01-05 09:01-05:01-05
                var startAt = fixedTime.AddDays(i - eventCount).AddMinutes(i - eventCount).AddSeconds(i - eventCount).UtcDateTime;

                var currentEvent = new Event(
                    new Guid($"E0000000-0000-0000-0000-00000000000{i}"),
                    $"{title} {i}",
                    $"{description} {i}",
                    startAt,
                    startAt.AddDays(eventCount),
                    i);

                allEvents.Add(currentEvent);
            }
        }

        private async Task AddAllEventsToDb()
        {
            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddRangeAsync(allEvents);
            await arrangeContext.SaveChangesAsync();
        }
    }
}
