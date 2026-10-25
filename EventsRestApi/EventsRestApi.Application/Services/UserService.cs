using EventsRestApi.Application.Dto.Request;
using EventsRestApi.Application.Exceptions;
using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Application.InterfacesForInfrastructure.Services;
using EventsRestApi.Application.Services.Interfaces;
using EventsRestApi.Application.Settings;
using EventsRestApi.Domain.Exceptions;
using EventsRestApi.Domain.Models.Entities;
using Microsoft.Extensions.Options;

namespace EventsRestApi.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IPasswordService _passwordService;

        private readonly ITokenService _tokenService;

        private readonly UserSettings _userSettings;

        public UserService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ITokenService tokenService,
            IOptions<UserSettings> userSettings)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _userSettings = userSettings.Value;
        }

        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (user == null)
            {
                throw new NotFoundResourceException<User>(id);
            }

            return user;
        }

        public async Task EnsureCanMakeBookingAsync(Guid userId, CancellationToken cancellationToken)
        {
            var activeBookingsCount = await _userRepository.GetActiveBookingsCountAsync(userId, cancellationToken);

            if (activeBookingsCount >= _userSettings.ActiveBookingLimit)
            {
                throw new ExceedingActiveBookingLimitException(userId, _userSettings.ActiveBookingLimit);
            }
        }

        public async Task<User> RegisterAsync(UserCredentialsDto userCredentialsDto, UserRole role, CancellationToken cancellationToken)
        {
            var foundUser = await _userRepository.GetByLoginAsync(userCredentialsDto.Login, cancellationToken);

            if (foundUser != null)
            {
                throw new UserAlreadyExistsException(userCredentialsDto.Login);
            }

            var hashedPassword = _passwordService.HashPassword(userCredentialsDto.Password);

            return await _userRepository.AddAsync(userCredentialsDto.Login, hashedPassword, role, cancellationToken);
        }

        public async Task<string> LogInAsync(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByLoginAsync(userCredentialsDto.Login, cancellationToken);

            if (user == null)
            {
                throw new InvalidCredentialsException();
            }

            _passwordService.VerifyPassword(userCredentialsDto.Password, user.HashedPassword);

            return _tokenService.GenerateToken(user.Id, user.Login, user.Role);
        }
    }
}
