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
    public class EventServiceTests
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 01, 09, 01, 01, TimeSpan.Zero);

        private readonly List<Event> allEvents = [];
        private readonly List<EventResponseDto> allEventResponseDto = [];

        private readonly EventService _eventService;
        private readonly Mock<IEventRepository> _mockEventRepository;

        public EventServiceTests()
        {
            FillTestData();

            var fakeTimeProvider = new FakeTimeProvider();
            fakeTimeProvider.SetUtcNow(fixedTime);

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
            List<EventResponseDto> expectedEventsDto = allEventResponseDto[2..4];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                allEventResponseDto[2..4],
                expectedEventsDto.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get(null, new DateTime(2026, 06, 3, 09, 03, 00), new DateTime(2026, 06, 9, 09, 04, 00), page, pageSize);

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
            List<EventResponseDto> expectedEventsDto = [allEventResponseDto[2]];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventResponseDto>(
                [allEventResponseDto[2]],
                expectedEventsDto.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository
                .Setup(mock => mock.Get())
                .Returns(allEvents);

            /*---ACT---*/
            var result = _eventService.Get("3", new DateTime(2026, 06, 3, 09, 03, 00), new DateTime(2026, 06, 9, 09, 04, 00), page, pageSize);

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
        public void GetAll_PageMoreThanTotalPages_ReturnEmptyPage()
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
        public void Add_CorrectEventDto_EventIdWasChanged()
        {
            /*---ARRANGE---*/
            var addingEventDto = new EventRequestDto(
                $"Event 6",
                $"Description 6",
                new DateTime(2026, 06, 06, 09, 06, 00),
                new DateTime(2026, 06, 06, 09, 06, 00).AddDays(5));

            var expectedEventDto = new EventResponseDto(
                new Guid($"00000000-0000-0000-0000-000000000006"),
                addingEventDto.Title,
                addingEventDto.Description,
                addingEventDto.StartAt,
                addingEventDto.EndAt);

            var expectedAddedEvent = Mapper.MapToEvent(addingEventDto);
            expectedAddedEvent.Id = expectedEventDto.Id;

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
            var id = allEvents[0].Id;

            var updatingEventDto = new EventRequestDto(
                $"Event New",
                $"Description 1",
                new DateTime(2026, 06, 01, 09, 01, 00),
                new DateTime(2026, 06, 01, 09, 01, 00).AddDays(5));

            _mockEventRepository
                .Setup(mock => mock.Update(It.IsAny<Event>()))
                .Returns(true);

            /*---ACT---*/
            var result = _eventService.Update(id, updatingEventDto);

            /*---ASSERT---*/
            Assert.True(result);
        }

        [Fact]
        public void Update_WithNonExistentId_ReturnFalse()
        {
            /*---ARRANGE---*/
            var id = Guid.Empty;

            var updatingEventDto = new EventRequestDto(
                $"Event New",
                $"Description 1",
                new DateTime(2026, 06, 01, 09, 01, 00),
                new DateTime(2026, 06, 01, 09, 01, 00).AddDays(5));

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
        public void CanCreateBooking_WithExistingAndNotEndedEvent_ReturnTrue()
        {
            /*---ARRANGE---*/
            var currentEvent = new Event(
                   new Guid($"00000000-0000-0000-0000-000000000001"),
                   $"Event 1",
                   $"Description 1",
                   new DateTime(2026, 06, 01, 09, 01, 01),
                   DateTime.UtcNow.AddDays(1));

            _mockEventRepository
                .Setup(mock => mock.GetById(currentEvent.Id))
                .Returns(currentEvent);

            /*---ACT---*/
            var result = _eventService.IsEventStillValid(currentEvent.Id);

            /*---ASSERT---*/
            Assert.True(result);
        }

        [Fact]
        public void CanCreateBooking_WithNonExistentEvent_ReturnFalse()
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
        public void CanCreateBooking_WithEndedEvent_ReturnFalse()
        {
            /*---ARRANGE---*/
            var currentEvent = new Event(
                   new Guid($"00000000-0000-0000-0000-000000000001"),
                   $"Event 1",
                   $"Description 1",
                   new DateTime(2026, 06, 01, 09, 01, 01),
                   DateTime.UtcNow.AddDays(-1));

            _mockEventRepository
                .Setup(mock => mock.GetById(currentEvent.Id))
                .Returns(currentEvent);

            /*---ACT---*/
            var result = _eventService.IsEventStillValid(currentEvent.Id);

            /*---ASSERT---*/
            Assert.False(result);
        }

        private void FillTestData()
        {
            var title = "Event";
            var description = "Description";
            var eventCount = 5;
            Event currentEvent;

            for (int i = 1; i <= eventCount; i++)
            {
                currentEvent = new Event(
                    new Guid($"00000000-0000-0000-0000-00000000000{i}"),
                    $"{title} {i}",
                    $"{description} {i}",
                    new DateTime(2026, 06, i % 28, 09, i % 60, 00),
                    new DateTime(2026, 06, i % 28, 09, i % 60, 00).AddDays(eventCount));

                allEvents.Add(currentEvent);

                allEventResponseDto.Add(Mapper.MapToEventResponseDto(currentEvent));
            }
        }
    }
}
