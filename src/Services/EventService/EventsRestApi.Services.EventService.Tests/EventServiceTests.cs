using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Cache;
using EventsRestApi.Services.EventService.Application.Models;
using EventsRestApi.Services.EventService.Application.Services.Interfaces;
using EventsRestApi.Services.EventService.Application.Settings;
using EventsRestApi.Services.EventService.Domain.Models.Entities;
using EventsRestApi.Services.EventService.Infrastructure.Data;
using EventsRestApi.Services.EventService.Infrastructure.Data.Repositories;
using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace EventsRestApi.Services.EventService.Tests
{
    public class EventServiceTests : IDisposable
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 05, 09, 05, 05, TimeSpan.Zero);
        private readonly List<Event> allEvents = [];
        private readonly EventDbContext _context;
        private readonly IEventService _eventService;
        private readonly Mock<ICacheService> _mockEventCacheService;

        public EventServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EventDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            _context = new EventDbContext(options);
            var _eventRepository = new EventRepository(_context);
            _mockEventCacheService = new Mock<ICacheService>();

            var cacheSettingsValue = new CacheSettings
            {
                EventByIdTtlMinutes = 5,
                TopEventsTtlMinutes = 10
            };

            _eventService = new EventsRestApi.Services.EventService.Application.Services.EventService(
                _mockEventCacheService.Object,
                _eventRepository,
                Options.Create(cacheSettingsValue),
                NullLogger<EventsRestApi.Services.EventService.Application.Services.EventService>.Instance
            );

            FillTestData();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetPageAsync Tests

        [Fact]
        public async Task GetPageAsync_NoParamsWithBigPageSize_ReturnAllEvents()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = 100;
            List<Event> expectedEvents = allEvents.OrderBy(x => x.StartAt).ToList();

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize);

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

            _context.Events.RemoveRange(_context.Events);
            await _context.SaveChangesAsync();

            List<Event> expectedEvents = [];

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize);

            /*---ACT---*/
            var result = await _eventService.GetPageAsync(null, null, null, page, pageSize, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        #endregion

        #region GetByIdAsync Tests

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

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<NotFoundResourceException<Event>>(async () =>
                await _eventService.GetByIdAsync(id, CancellationToken.None));
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
        public async Task GetByIdAsync_WhenCacheHit_ReturnFromCacheWithoutDb()
        {
            /*---ARRANGE---*/
            var startAt = DateTime.UtcNow;

            var cachedEvent = new Event(
                new Guid($"E0000000-0000-0000-0000-000000000010"),
                $"Event 1",
                $"Description 1",
                startAt,
                startAt.AddDays(5),
                1);

            var cacheKey = $"{CacheKeys.GetEventById}{cachedEvent.Id}";

            _mockEventCacheService
                .Setup(x => x.GetAsync(cacheKey))
                .ReturnsAsync(JsonSerializer.Serialize(cachedEvent));

            /*---ACT---*/
            var result = await _eventService.GetByIdAsync(cachedEvent.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(cachedEvent, result);

            var dbCheck = await _context.Events.FirstOrDefaultAsync(x => x.Id == cachedEvent.Id);
            Assert.Null(dbCheck);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheMiss_GetFromDbAndSaveToCache()
        {
            /*---ARRANGE---*/
            var dbEvent = allEvents[0];
            var cacheKey = $"{CacheKeys.GetEventById}{dbEvent.Id}";

            _mockEventCacheService.Setup(x => x.GetAsync(cacheKey))
                .ReturnsAsync((string?)null);

            /*---ACT---*/
            var result = await _eventService.GetByIdAsync(dbEvent.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equivalent(dbEvent, result, true);

            _mockEventCacheService.Verify(x => x.SetAsync(
                cacheKey,
                It.Is<string>(json => json.Contains(dbEvent.Title)),
                TimeSpan.FromMinutes(5)),
                Times.Once);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_CorrectEventDto_ReturnGeneratedEvent()
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
            //Дата startAt высчитывается относительно fixedTime: 2026.06.05 09:05:05
            var startAt = fixedTime.UtcDateTime;
            var endAt = startAt.AddDays(5);

            var @event = new Event(Guid.Empty, string.Empty, string.Empty, startAt, endAt, 1);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _eventService.AddAsync(@event, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithExistingId_ReturnTrue()
        {
            /*---ARRANGE---*/
            var updatingEvent = allEvents[0];

            var updatedEvent = new Event(
                updatingEvent.Id,
                updatingEvent.Title + "New",
                updatingEvent.Description + "New",
                updatingEvent.StartAt.AddDays(-1),
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
        public async Task UpdateAsync_EventWasUpdated_UpdateDbAndDeleteFromCache()
        {
            /*---ARRANGE---*/
            var dbEvent = allEvents[0];
            var cacheKey = $"{CacheKeys.GetEventById}{dbEvent.Id}";

            dbEvent.Title = dbEvent.Title + "New";

            /*---ACT---*/
            await _eventService.UpdateAsync(dbEvent, CancellationToken.None);

            /*---ASSERT---*/
            var result = await _context.Events.FirstOrDefaultAsync(x => x.Id == dbEvent.Id);

            Assert.NotNull(result);
            Assert.Equivalent(dbEvent, result, true);

            _mockEventCacheService.Verify(x => x.DeleteAsync(cacheKey), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CancelBeforeStart_ThrowOperationCancelledException()
        {
            /*---ARRANGE---*/
            //Дата startAt высчитывается относительно fixedTime: 2026.06.05 09:05:05
            var startAt = fixedTime.UtcDateTime;
            var endAt = startAt.AddDays(5);

            var @event = new Event(Guid.Empty, string.Empty, string.Empty, startAt, endAt, 1);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            /*---ACT & ASSERT---*/
            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _eventService.UpdateAsync(@event, cts.Token));

            Assert.Equal(cts.Token, exception.CancellationToken);
        }

        #endregion

        #region DeleteAsync Tests

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
        public async Task DeleteAsync_EventWasDeleted_DeleteFromDbAndDeleteFromCache()
        {
            /*---ARRANGE---*/
            var dbEvent = allEvents[0];
            var cacheKey = $"{CacheKeys.GetEventById}{dbEvent.Id}";

            /*---ACT---*/
            await _eventService.DeleteAsync(dbEvent.Id, CancellationToken.None);

            /*---ASSERT---*/
            var eventInDb = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dbEvent.Id);

            Assert.Null(eventInDb);

            _mockEventCacheService.Verify(x => x.DeleteAsync(cacheKey), Times.Once);
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

        #endregion

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
