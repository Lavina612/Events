using EventsRestApi.Services.SharedContracts.Controllers;
using EventsRestApi.Services.SharedContracts.Models;
using EventsRestApi.Services.UserService.Application.Dto.Request;
using EventsRestApi.Services.UserService.Application.Dto.Response;
using EventsRestApi.Services.UserService.Application.Mappers;
using EventsRestApi.Services.UserService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsRestApi.Services.UserService.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id, CancellationToken cancellationToken = default)
        {
            var foundUser = await _userService.GetByIdAsync(id, cancellationToken);

            if (foundUser == null)
            {
                return Problem(
                    detail: $"Пользователь с Id: {id} не найден.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(UserMapper.MapToUserResponseDto(foundUser));
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDto>> Register(UserCredentialsDto userCredentialsDto, UserRole role = UserRole.User, CancellationToken cancellationToken = default)
        {
            var registeredUser = await _userService.RegisterAsync(userCredentialsDto, role, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, UserMapper.MapToUserResponseDto(registeredUser));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenResponseDto>> LogIn(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken = default)
        {
            var token = await _userService.LogInAsync(userCredentialsDto, cancellationToken);

            return Ok(new TokenResponseDto(token));
        }
    }
}
