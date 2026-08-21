using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using EventsRestApi.Models;
using EventsRestApi.Services;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace EventsRestApi.Tests
{
    public class EventServiceTests
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 05, 09, 05, 05, TimeSpan.Zero);
        private readonly List<Event> allEvents = [];
        private readonly EventService _eventService;
        private readonly Mock<IEventRepository> _mockEventRepository;

        public EventServiceTests()
        {
            var fakeTimeProvider = new FakeTimeProvider();
            fakeTimeProvider.SetUtcNow(fixedTime);

            FillTestData();

            _mockEventRepository = new Mock<IEventRepository>();

            _eventService = new EventService(_mockEventRepository.Object);
        }

        [Fact]
        public void GetAll_NoParamsWithMaxPageSize_ReturnAllEvents()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = int.MaxValue;
            List<Event> expectedEvents = allEvents;
            var totalPages = (expectedEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get(null, null, null, page, pageSize);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithTitleParam_ReturnFilteredByTitleIgnoreCase()
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

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get(titleParam, null, null, page, pageSize);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithStartAtAndEndAtParams_ReturnFilteredBetweenDates()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = int.MaxValue;

            //Дата startAt высчитывается относительно fixedTime: 2026.06.03 09:03:00
            var startAt = fixedTime.AddDays(-2).AddMinutes(-2).AddSeconds(-5).UtcDateTime;
            var endAt = startAt.AddDays(6).AddMinutes(4).AddSeconds(4);

            List<Event> expectedEvents = allEvents[2..4];
            var totalPages = (expectedEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                expectedEvents.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get(null, startAt, endAt, page, pageSize);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithTitleAndStartAtAndEndAtParams_ReturnFilteredByTitleAndBetweenDates()
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

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get("3", startAt, endAt, page, pageSize);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithPageAndPageSize_ReturnEventsForPage()
        {
            /*---ARRANGE---*/
            var page = 2;
            var pageSize = 2;
            List<Event> expectedEvents = allEvents[2..4];
            var totalPages = (allEvents.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<Event>(
                expectedEvents,
                allEvents.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get(null, null, null, page, pageSize);

            /*---ASSERT---*/
            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_NoEvents_ReturnEmptyPage()
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

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns([]);

            /*---ACT---*/
            var result = _eventService.Get(null, null, null, page, pageSize);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithPageMoreThanTotalPages_ReturnEmptyPage()
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

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get(null, null, null, page, pageSize);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetById_WithExistingId_ReturnEventById()
        {
            /*---ARRANGE---*/
            var id = allEvents[2].Id;
            var expected = allEvents[2];

            _mockEventRepository
                .Setup(mock => mock.GetById(id))
                .Returns(allEvents[2]);

            /*---ACT---*/
            var result = _eventService.GetById(id);

            /*---ASSERT---*/
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetById_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            var id = Guid.Empty;

            _mockEventRepository
                .Setup(mock => mock.GetById(id))
                .Returns((Event?)null);

            /*---ACT---*/
            var result = _eventService.GetById(id);

            /*---ASSERT---*/
            Assert.Null(result);
        }

        [Fact]
        public void Add_CorrectEventDto_EventIdWasGenerated()
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

            var expectedAddedEvent = new Event(
                new Guid("E0000000-0000-0000-0000-000000000006"),
                addingEvent.Title,
                addingEvent.Description,
                addingEvent.StartAt,
                addingEvent.EndAt,
                addingEvent.TotalSeats);

            _mockEventRepository
                .Setup(mock => mock.Add(It.IsAny<Event>()))
                .Returns(expectedAddedEvent);

            /*---ACT---*/
            var result = _eventService.Add(addingEvent);

            /*---ASSERT---*/
            Assert.Equivalent(expectedAddedEvent, result, true);
        }

        [Fact]
        public void Update_WithExistingId_ReturnTrue()
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

            _mockEventRepository
                .Setup(mock => mock.Update(updatedEvent))
                .Returns(true);

            /*---ACT---*/
            var result = _eventService.Update(updatedEvent);

            /*---ASSERT---*/
            Assert.True(result);
        }

        [Fact]
        public void Update_WithNonExistentId_ReturnFalse()
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

            _mockEventRepository
                .Setup(mock => mock.Update(updatingEvent))
                .Returns(false);

            /*---ACT---*/
            var result = _eventService.Update(updatingEvent);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public void Delete_WithExistingId_ReturnTrue()
        {
            /*---ARRANGE---*/
            var id = allEvents.Last().Id;

            _mockEventRepository
                .Setup(mock => mock.Delete(id))
                .Returns(true);

            /*---ACT---*/
            var result = _eventService.Delete(id);

            /*---ASSERT---*/
            Assert.True(result);
        }

        [Fact]
        public void Delete_WithNonExistentId_ReturnFalse()
        {
            /*---ARRANGE---*/
            var id = Guid.Empty;

            _mockEventRepository
                .Setup(mock => mock.Delete(id))
                .Returns(false);

            /*---ACT---*/
            var result = _eventService.Delete(id);

            /*---ASSERT---*/
            Assert.False(result);
        }

        private void FillTestData()
        {
            var title = "Event";
            var description = "Description";
            var eventCount = 5;

            for (int i = 1; i <= eventCount; i++)
            {
                //Дата startAt высчитывается относительно fixedTime: 2026.06.01-05 09:01-05:01-05
                var startAt = fixedTime.UtcDateTime.AddDays(i - eventCount).AddMinutes(i - eventCount).AddSeconds(i - eventCount);

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
    }
}
