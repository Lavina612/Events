using EventsRestApi.Dto.Request;
using EventsRestApi.Dto.Response;
using EventsRestApi.Interfaces;
using EventsRestApi.Mappers;
using EventsRestApi.Models;
using EventsRestApi.Services;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace EventsRestApi.Tests
{
    //TODO: Fix all tests
    public class EventServiceTests
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 05, 09, 05, 05, TimeSpan.Zero);

        private readonly List<Event> allEvents = [];
        private readonly List<EventResponseDto> allEventResponseDto = [];

        private readonly EventService _eventService;
        private readonly Mock<IEventRepository> _mockEventRepository;

        public EventServiceTests()
        {
            var fakeTimeProvider = new FakeTimeProvider();
            fakeTimeProvider.SetUtcNow(fixedTime);

            FillTestData();

            _mockEventRepository = new Mock<IEventRepository>();

            _eventService = new EventService(_mockEventRepository.Object, fakeTimeProvider);
        }

        [Fact]
        public void GetAll_NoParamsWithMaxPageSize_ReturnAllEvents()
        {
            /*---ARRANGE---*/
            var page = 1;
            var pageSize = int.MaxValue;
            List<EventResponseDto> expectedEventsDto = allEventResponseDto;
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                expectedEventsDto,
                expectedEventsDto.Count,
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
            List<EventResponseDto> expectedEventsDto = [allEventResponseDto[0]];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                expectedEventsDto,
                expectedEventsDto.Count,
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

            List<EventResponseDto> expectedEventsDto = allEventResponseDto[2..4];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                expectedEventsDto,
                expectedEventsDto.Count,
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

            List<EventResponseDto> expectedEventsDto = [allEventResponseDto[2]];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                expectedEventsDto,
                expectedEventsDto.Count,
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
            List<EventResponseDto> expectedEventsDto = allEventResponseDto[2..4];
            var totalPages = (allEventResponseDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                expectedEventsDto,
                allEventResponseDto.Count,
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
            List<EventResponseDto> expectedEventsDto = [];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                expectedEventsDto,
                expectedEventsDto.Count,
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
            List<EventResponseDto> expectedEventsDto = [];
            var totalPages = (allEventResponseDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                expectedEventsDto,
                allEventResponseDto.Count,
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
            var id = allEventResponseDto[2].Id;
            var expected = allEventResponseDto[2];

            _mockEventRepository
                .Setup(mock => mock.GetById(id))
                .Returns(allEvents[2]);

            /*---ACT---*/
            var result = _eventService.GetDtoById(id);

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
            var result = _eventService.GetDtoById(id);

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

            var addingEventDto = new EventRequestDto(
                $"Event 6",
                $"Description 6",
                startAt,
                endAt,
                6);

            var expectedEventDto = new EventResponseDto(
                new Guid($"E0000000-0000-0000-0000-000000000006"),
                addingEventDto.Title,
                addingEventDto.Description,
                addingEventDto.StartAt,
                addingEventDto.EndAt,
                addingEventDto.TotalSeats,
                addingEventDto.TotalSeats);

            var expectedAddedEvent = Mapper.MapToEvent(expectedEventDto.Id, addingEventDto);

            _mockEventRepository
                .Setup(mock => mock.Add(It.IsAny<Event>()))
                .Returns(expectedAddedEvent);

            /*---ACT---*/
            var result = _eventService.Add(addingEventDto);

            /*---ASSERT---*/
            Assert.Equivalent(expectedEventDto, result, true);
        }

        [Fact]
        public void Update_WithExistingId_ReturnTrue()
        {
            /*---ARRANGE---*/
            var updatingEvent = allEvents[0];

            var updatingEventDto = new EventRequestDto(
                updatingEvent.Title + "New",
                updatingEvent.Description + "New",
                updatingEvent.StartAt.AddDays(1),
                updatingEvent.EndAt.AddDays(1),
                updatingEvent.TotalSeats);

            _mockEventRepository
                .Setup(mock => mock.Update(It.IsAny<Event>()))
                .Returns(true);

            /*---ACT---*/
            var result = _eventService.Update(updatingEvent.Id, updatingEventDto);

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

            var updatingEventDto = new EventRequestDto(
                $"Event New",
                $"Description 1",
                startAt,
                endAt,
                1);

            _mockEventRepository
                .Setup(mock => mock.Update(It.IsAny<Event>()))
                .Returns(false);

            /*---ACT---*/
            var result = _eventService.Update(id, updatingEventDto);

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

        [Fact]
        public void IsEventStillValid_WithExistingAndNotEndedEvent_ReturnTrue()
        {
            /*---ARRANGE---*/
            var currentEvent = allEvents[0];

            _mockEventRepository
                .Setup(mock => mock.GetById(currentEvent.Id))
                .Returns(currentEvent);

            /*---ACT---*/
            var result = _eventService.IsEventStillValid(currentEvent.Id);

            /*---ASSERT---*/
            Assert.True(result);
        }

        [Fact]
        public void IsEventStillValid_WithNonExistentEvent_ReturnFalse()
        {
            /*---ARRANGE---*/
            var id = Guid.Empty;

            _mockEventRepository
                .Setup(mock => mock.GetById(id))
                .Returns((Event?)null);

            /*---ACT---*/
            var result = _eventService.IsEventStillValid(id);

            /*---ASSERT---*/
            Assert.False(result);
        }

        [Fact]
        public void IsEventStillValid_WithEndedEvent_ReturnFalse()
        {
            /*---ARRANGE---*/
            var endedEvent = new Event(
                allEvents[0].Id,
                allEvents[0].Title,
                allEvents[0].Description,
                allEvents[0].StartAt,
                fixedTime.AddDays(-1).UtcDateTime,
                allEvents[0].TotalSeats);

            _mockEventRepository
                .Setup(mock => mock.GetById(endedEvent.Id))
                .Returns(endedEvent);

            /*---ACT---*/
            var result = _eventService.IsEventStillValid(endedEvent.Id);

            /*---ASSERT---*/
            Assert.False(result);
        }

        private void FillTestData()
        {
            var title = "Event";
            var description = "Description";
            var eventCount = 5;
            Event currentEvent;
            DateTime startAt;

            for (int i = 1; i <= eventCount; i++)
            {
                //Дата startAt высчитывается относительно fixedTime: 2026.06.01-05 09:01-05:01-05
                startAt = fixedTime.UtcDateTime.AddDays(i - eventCount).AddMinutes(i - eventCount).AddSeconds(i - eventCount);

                currentEvent = new Event(
                    new Guid($"E0000000-0000-0000-0000-00000000000{i}"),
                    $"{title} {i}",
                    $"{description} {i}",
                    startAt,
                    startAt.AddDays(eventCount),
                    i);

                allEvents.Add(currentEvent);

                allEventResponseDto.Add(Mapper.MapToEventResponseDto(currentEvent));
            }
        }
    }
}
