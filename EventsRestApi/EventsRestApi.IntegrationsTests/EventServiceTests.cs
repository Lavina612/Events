using EventsRestApi.Data.Repositories;
using EventsRestApi.Dto.Response;
using EventsRestApi.Models;
using EventsRestApi.Services;

namespace EventsRestApi.IntegrationsTests
{
    public class EventServiceTests : BaseDbTest
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 05, 09, 05, 05, TimeSpan.Zero);
        private readonly List<Event> allEvents = [];

        public EventServiceTests(DbFixture fixture) : base(fixture)
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
            var eventService = new EventService(new EventRepository(actContext));
            var result = await eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

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
            var eventService = new EventService(new EventRepository(actContext));
            var result = await eventService.GetPageAsync(titleParam, null, null, page, pageSize, CancellationToken.None);

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
            var eventService = new EventService(new EventRepository(actContext));
            var result = await eventService.GetPageAsync(null, startAt, endAt, page, pageSize, CancellationToken.None);

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
            var eventService = new EventService(new EventRepository(actContext));
            var result = await eventService.GetPageAsync("3", startAt, endAt, page, pageSize, CancellationToken.None);

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
            var eventService = new EventService(new EventRepository(actContext));
            var result = await eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

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
            var eventService = new EventService(new EventRepository(actContext));
            var result = await eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

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
            var eventService = new EventService(new EventRepository(actContext));
            var result = await eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

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
            var eventService = new EventService(new EventRepository(actContext));

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await eventService.GetPageAsync(string.Empty, default(DateTime), default(DateTime), 1, 100, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
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
