using EventsRestApi.Data;
using EventsRestApi.Data.Repositories;
using EventsRestApi.Data.Repositories.Interfaces;
using EventsRestApi.Dto.Response;
using EventsRestApi.Models;
using EventsRestApi.Services;
using EventsRestApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventsRestApi.Tests
{
    public class EventServiceTests : IDisposable
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 05, 09, 05, 05, TimeSpan.Zero);
        private readonly List<Event> allEvents = [];
        private readonly ServiceProvider _serviceProvider;
        private readonly AppDbContext _context;
        private readonly IEventService _eventService;

        public EventServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService>();

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<AppDbContext>();
            _eventService = _serviceProvider.GetRequiredService<IEventService>();

            FillTestData();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _serviceProvider.Dispose();
        }

        [Fact]
        public async Task GetPageAsync_NoParamsWithMaxPageSize_ReturnAllEvents()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = int.MaxValue;
            List<Event> expectedEvents = allEvents.OrderBy(x => x.StartAt).ToList();
            var totalPages = (expectedEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize,
                totalPages);

            /*---ACT---*/
            var result = await _eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithTitleParam_ReturnFilteredByTitleIgnoreCase()
        {
            /*---ARRANGE---*/
            var titleParam = allEvents[0].Title.Substring(2).ToUpper();
            var page = 1;
            var pageSize = int.MaxValue;
            List<Event> expectedEvents = [allEvents[0]];
            var totalPages = (expectedEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize,
                totalPages);

            /*---ACT---*/
            var result = await _eventService.GetPageAsync(titleParam, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithStartAtAndEndAtParams_ReturnFilteredBetweenDates()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = int.MaxValue;

            //Дата startAt высчитывается относительно fixedTime: 2026.06.03 09:03:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(-2).AddSeconds(-5).UtcDateTime;
            var endAt = startAt.AddDays(6).AddMinutes(4).AddSeconds(4);

            List<Event> expectedEvents = allEvents[2..4].OrderBy(x => x.StartAt).ToList();
            var totalPages = (expectedEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize,
                totalPages);

            /*---ACT---*/
            var result = await _eventService.GetPageAsync(null, startAt, endAt, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithTitleAndStartAtAndEndAtParams_ReturnFilteredByTitleAndBetweenDates()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = int.MaxValue;

            //Дата startAt высчитывается относительно fixedTime: 2026.06.03 09:03:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(-2).AddSeconds(-5).UtcDateTime;
            var endAt = startAt.AddDays(6).AddMinutes(4).AddSeconds(4);

            List<Event> expectedEvents = [allEvents[2]];
            var totalPages = (expectedEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize,
                totalPages);

            /*---ACT---*/
            var result = await _eventService.GetPageAsync("3", startAt, endAt, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithPageAndPageSize_ReturnEventsForPage()
        {
            /*---ARRANGE---*/
            var page = 2;
            var pageSize = 2;
            List<Event> expectedEvents = allEvents.OrderBy(x => x.StartAt).Skip(2).Take(2).ToList();
            var totalPages = (allEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                allEvents.Count,
                page,
                pageSize,
                totalPages);

            /*---ACT---*/
            var result = await _eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_NoEvents_ReturnEmptyPage()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = 10;
            List<Event> expectedEvents = [];
            var totalPages = (expectedEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize,
                totalPages);

            /*---ACT---*/
            _context.Events.RemoveRange(_context.Events);
            _context.SaveChanges();

            var result = await _eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_WithPageMoreThanTotalPages_ReturnEmptyPage()
        {
            /*---ARRANGE---*/
            var page = 10;
            var pageSize = 10;
            List<Event> expectedEvents = [];
            var totalPages = (allEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                allEvents.Count,
                page,
                pageSize,
                totalPages);

            /*---ACT---*/
            var result = await _eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetPageAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _eventService.GetPageAsync(string.Empty, default(DateTime), default(DateTime), 0, 0, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnEventById()
        {
            /*---ARRANGE---*/
            var id = allEvents[2].Id;
            var expected = allEvents[2];

            /*---ACT---*/
            var result = await _eventService.GetByIdAsync(id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            var id = Guid.Empty;

            /*---ACT---*/
            var result = await _eventService.GetByIdAsync(id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _eventService.GetByIdAsync(Guid.Empty, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        [Fact]
        public async Task AddAsync_CorrectEventDto_EventIdWasGenerated()
        {
            /*---ARRANGE---*/
            //Дата startAt высчитывается относительно fixedTime: 2026.06.06 09:06:00
            var startAt = fixedTime.AddDays(1).AddMinutes(1).UtcDateTime;
            var endAt = startAt.AddDays(5);

            var addingEvent = new Event(
                Guid.Empty,
                "Event 6",
                "Description 6",
                startAt,
                endAt,
                6);

            /*---ACT---*/
            var result = await _eventService.AddAsync(addingEvent, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotEqual(Guid.Empty, result.Id);
            result.Should().BeEquivalentTo(addingEvent, options => options.Excluding(x => x.Id));
        }

        [Fact]
        public async Task AddAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            var @event = new Event(Guid.Empty, string.Empty, string.Empty, DateTime.UtcNow, DateTime.UtcNow, 1);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _eventService.AddAsync(@event, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        [Fact]
        public async Task UpdateAsync_WithExistingId_ReturnTrue()
        {
            /*---ARRANGE---*/
            var updatingEvent = allEvents[0];

            var updatedEvent = new Event(
                updatingEvent.Id,
                updatingEvent.Title + "New",
                updatingEvent.Description + "New",
                updatingEvent.StartAt.AddDays(1),
                updatingEvent.EndAt.AddDays(1),
                updatingEvent.TotalSeats);

            /*---ACT---*/
            var result = await _eventService.UpdateAsync(updatedEvent, CancellationToken.None);

            /*---ASSERT---*/
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnFalse()
        {
            /*---ARRANGE---*/
            var id = Guid.Empty;

            //Дата startAt высчитывается относительно fixedTime: 2026.06.01 09:01:00
            var startAt = fixedTime.AddDays(-4).AddMinutes(-4).UtcDateTime;
            var endAt = startAt.AddDays(5);

            var updatingEvent = new Event(
                id,
                "Event New",
                "Description 1",
                startAt,
                endAt,
                1);

            /*---ACT---*/
            var result = await _eventService.UpdateAsync(updatingEvent, CancellationToken.None);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            var @event = new Event(Guid.Empty, string.Empty, string.Empty, DateTime.UtcNow, DateTime.UtcNow, 1);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _eventService.UpdateAsync(@event, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_ReturnTrue()
        {
            /*---ARRANGE---*/
            var id = allEvents.Last().Id;

            /*---ACT---*/
            var result = await _eventService.DeleteAsync(id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnFalse()
        {
            /*---ARRANGE---*/
            var id = Guid.Empty;

            /*---ACT---*/
            var result = await _eventService.DeleteAsync(id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _eventService.DeleteAsync(Guid.Empty, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        private void FillTestData()
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

            _context.Events.AddRange(allEvents);
            _context.SaveChanges();
        }
    }
}
