using EventsRestApi.Services.EventService.Application.Dto.Request;
using EventsRestApi.Services.EventService.Application.Dto.Response;
using EventsRestApi.Services.EventService.Application.Mappers;
using EventsRestApi.Services.EventService.Application.Services.Interfaces;
using EventsRestApi.Services.SharedContracts.Controllers;
using EventsRestApi.Services.SharedContracts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventsRestApi.Services.EventService.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class EventsController : BaseApiController
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedResult<EventResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<EventResponseDto>>> GetAll(
            [FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть положительным.")] int page = 1,
            [FromQuery][Range(1, 100, ErrorMessage = "Размер страницы должен быть от 1 до 100.")] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var resultWithEvents = await _eventService.GetPageAsync(title, from, to, page, pageSize, cancellationToken);

            return Ok(new PaginatedResult<EventResponseDto>(
                resultWithEvents.ItemsForPage.Select(EventMapper.MapToEventResponseDto).ToList(),
                resultWithEvents.TotalCount,
                resultWithEvents.Page,
                resultWithEvents.PageSize));
        }

        [HttpGet("top")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<EventResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<EventResponseDto>>> GetTop10(CancellationToken cancellationToken = default)
        {
            var topEvents = await _eventService.GetTop10PopularAsync(cancellationToken);

            return Ok(topEvents.Select(EventMapper.MapToEventResponseDto).ToList());
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EventResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var foundEvent = await _eventService.GetByIdAsync(id, cancellationToken);

            return Ok(EventMapper.MapToEventResponseDto(foundEvent));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EventResponseDto>> Add(EventRequestDto addingEventDto, CancellationToken cancellationToken = default)
        {
            var addedEvent = await _eventService.AddAsync(EventMapper.MapToEvent(addingEventDto), cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = addedEvent.Id }, EventMapper.MapToEventResponseDto(addedEvent));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, EventRequestDto updatingEventDto, CancellationToken cancellationToken = default)
        {
            var isUpdated = await _eventService.UpdateAsync(EventMapper.MapToEvent(id, updatingEventDto), cancellationToken);

            if (!isUpdated)
            {
                return Problem(
                    detail: $"Событие с Id: {id} не найдено.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var isDeleted = await _eventService.DeleteAsync(id, cancellationToken);

            if (!isDeleted)
            {
                return Problem(
                    detail: $"Событие с Id: {id} не найдено.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return NoContent();
        }
    }
}
