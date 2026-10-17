using EventsRestApi.Data.Repositories;
using EventsRestApi.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventsRestApi.IntegrationsTests
{
    public class EventRepositoryTests : BaseDbTest
    {
        public EventRepositoryTests(DbFixture fixture) : base(fixture) { }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_TwoEventsInDb_ReturnAllEvents()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var event1 = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                new DateTime(2026, 06, 01, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 06, 00, 00, 00, DateTimeKind.Utc),
                1);

            var event2 = new Event(
                new Guid("E0000000-0000-0000-0000-000000000002"),
                "Event 2",
                "Description 2",
                new DateTime(2026, 06, 02, 00, 00, 00, DateTimeKind.Utc),
                new DateTime(2026, 06, 07, 00, 00, 00, DateTimeKind.Utc),
                2);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Events.AddRangeAsync(event1, event2);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetAll().ToListAsync();

            /*---ASSERT---*/
            Assert.Equal(2, result.Count);
            result.Should().BeEquivalentTo([event1, event2]);
        }

        [Fact]
        public async Task GetAll_NoEventsInDb_ReturnEmptyCollection()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var eventRepository = new EventRepository(actContext);
            var result = await eventRepository.GetAll().ToListAsync();

            /*---ASSERT---*/
            Assert.Empty(result);
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
    }
}
