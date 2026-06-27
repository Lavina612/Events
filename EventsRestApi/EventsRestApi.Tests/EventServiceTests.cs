using EventsRestApi.Dto;
using EventsRestApi.Interfaces;
using EventsRestApi.Models;
using EventsRestApi.Services;
using Moq;

namespace EventsRestApi.Tests
{
    public class EventServiceTests
    {
        private readonly EventService _eventService;
        private readonly Mock<IEventRepository> _mockEventRepository;

        private readonly List<Event> allEvents = [];
        private readonly List<EventDto> allEventsDto = [];

        public EventServiceTests()
        {
            FillTestData();

            _mockEventRepository = new Mock<IEventRepository>();
            _eventService = new EventService(_mockEventRepository.Object);
        }

        [Fact]
        public void GetAll_NoParamsWithMaxPageSize_ReturnAllEvents()
        {
            var page = 1;
            var pageSize = int.MaxValue;
            List<EventDto> expectedEventsDto = allEventsDto;
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventDto>(
                expectedEventsDto,
                expectedEventsDto.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository.Setup(mock => mock.Get()).Returns(allEvents);

            var result = _eventService.Get(null, null, null, page, pageSize);

            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithTitleParam_ReturnFilteredByTitle()
        {
            var page = 1;
            var pageSize = int.MaxValue;
            List<EventDto> expectedEventsDto = [allEventsDto[0]];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventDto>(
                expectedEventsDto,
                expectedEventsDto.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository.Setup(mock => mock.Get()).Returns(allEvents);

            var result = _eventService.Get("1", null, null, page, pageSize);

            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithStartAtAndEndAtParams_ReturnFilteredBetweenDates()
        {
            var page = 1;
            var pageSize = int.MaxValue;
            List<EventDto> expectedEventsDto = allEventsDto[2..4];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventDto>(
                allEventsDto[2..4],
                expectedEventsDto.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository.Setup(mock => mock.Get()).Returns(allEvents);

            var result = _eventService.Get(null, new DateTime(2026, 06, 3, 09, 03, 00), new DateTime(2026, 06, 9, 09, 04, 00), page, pageSize);

            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithTitleAndStartAtAndEndAtParams_ReturnFilteredByTitleAndBetweenDates()
        {
            var page = 1;
            var pageSize = int.MaxValue;
            List<EventDto> expectedEventsDto = [allEventsDto[2]];
            var totalPages = (expectedEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventDto>(
                [allEventsDto[2]],
                expectedEventsDto.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository.Setup(mock => mock.Get()).Returns(allEvents);

            var result = _eventService.Get("3", new DateTime(2026, 06, 3, 09, 03, 00), new DateTime(2026, 06, 9, 09, 04, 00), page, pageSize);

            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetAll_WithPageAndPageSize_ReturnEventsForPage()
        {
            var page = 2;
            var pageSize = 2;
            List<EventDto> expectedEventsDto = allEventsDto[2..4];
            var totalPages = (allEventsDto.Count + pageSize - 1) / pageSize;

            var expected = new PaginatedResult<EventDto>(
                expectedEventsDto,
                allEventsDto.Count,
                page,
                pageSize,
                totalPages);

            _mockEventRepository.Setup(mock => mock.Get()).Returns(allEvents);

            var result = _eventService.Get(null, null, null, page, pageSize);

            Assert.Equal(expected.ItemsForPage.Select(x => x.Id), result.ItemsForPage.Select(x => x.Id));
            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetById_WithExistingId_ReturnEventById()
        {
            var id = 3;
            var expected = allEventsDto[2];

            _mockEventRepository.Setup(mock => mock.GetById(id)).Returns(allEvents[2]);

            var result = _eventService.GetById(id);

            Assert.Equivalent(expected, result, true);
        }

        [Fact]
        public void GetById_WithNonExistentId_ReturnNull()
        {
            var id = 0;

            _mockEventRepository.Setup(mock => mock.GetById(id)).Returns((Event?)null);

            var result = _eventService.GetById(id);

            Assert.Null(result);
        }

        [Fact]
        public void Add_CorrectEventDto_EventIdWasChanged()
        {
            var addingEventDto = new EventDto(
                0,
                $"Event 6",
                $"Description 6",
                new DateTime(2026, 06, 06, 09, 06, 00),
                new DateTime(2026, 06, 06, 09, 06, 00).AddDays(5));

            var expectedEventDto = new EventDto(
                6,
                addingEventDto.Title,
                addingEventDto.Description,
                addingEventDto.StartAt,
                addingEventDto.EndAt);

            _mockEventRepository.Setup(mock => mock.Get()).Returns(allEvents);

            var result = _eventService.Add(addingEventDto);

            Assert.Equivalent(expectedEventDto, result, true);
        }

        [Fact]
        public void Update_WithExistingId_ReturnTrue()
        {
            var id = 1;

            var updatingEventDto = new EventDto(
                1,
                $"Event New",
                $"Description 1",
                new DateTime(2026, 06, 01, 09, 01, 00),
                new DateTime(2026, 06, 01, 09, 01, 00).AddDays(5));

            _mockEventRepository.Setup(mock => mock.Update(id, It.IsAny<Event>())).Returns(true);

            var result = _eventService.Update(id, updatingEventDto);

            Assert.True(result);
        }

        [Fact]
        public void Update_WithNonExistentId_ReturnFalse()
        {
            var id = 0;

            var updatingEventDto = new EventDto(
                0,
                $"Event New",
                $"Description 1",
                new DateTime(2026, 06, 01, 09, 01, 00),
                new DateTime(2026, 06, 01, 09, 01, 00).AddDays(5));

            _mockEventRepository.Setup(mock => mock.Update(id, It.IsAny<Event>())).Returns(false);

            var result = _eventService.Update(id, updatingEventDto);

            Assert.False(result);
        }

        [Fact]
        public void Delete_WithExistingId_ReturnTrue()
        {
            var id = allEvents.Last().Id;

            _mockEventRepository.Setup(mock => mock.Delete(id)).Returns(true);

            var result = _eventService.Delete(id);

            Assert.True(result);
        }

        [Fact]
        public void Delete_WithNonExistentId_ReturnFalse()
        {
            var id = 0;

            _mockEventRepository.Setup(mock => mock.Delete(id)).Returns(false);

            var result = _eventService.Delete(id);

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
                    i,
                    $"{title} {i}",
                    $"{description} {i}",
                    new DateTime(2026, 06, i % 28, 09, i % 60, 00),
                    new DateTime(2026, 06, i % 28, 09, i % 60, 00).AddDays(eventCount));

                allEvents.Add(currentEvent);

                allEventsDto.Add(EventMapper.MapToEventDto(currentEvent));
            }
        }
    }
}
