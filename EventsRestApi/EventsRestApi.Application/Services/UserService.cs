using EventsRestApi.Application.Dto.Request;
using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Application.InterfacesForInfrastructure.Services;
using EventsRestApi.Application.Services.Interfaces;
using EventsRestApi.Application.Settings;
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

        public async Task<bool> CanMakeBooking(Guid userId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetBookingsCountAsync(userId, cancellationToken) < _userSettings.ActiveBookingLimit;
        }

        public async Task<User> Register(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken)
        {
            var foundUser = await _userRepository.GetByLoginAsync(userCredentialsDto.Login, cancellationToken);

            if (foundUser != null)
            {
                throw new InvalidOperationException($"Пользователь с логином {userCredentialsDto.Login} уже существует.");
            }

            var hashedPassword = _passwordService.HashPassword(userCredentialsDto.Password);

            return await _userRepository.AddAsync(userCredentialsDto.Login, hashedPassword, cancellationToken);
        }

        public async Task<string> LogIn(UserCredentialsDto userCredentialsDto, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByLoginAsync(userCredentialsDto.Login, cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Неверный логин или пароль.");
            }

            _passwordService.VerifyPassword(userCredentialsDto.Password, user.HashPassword);

            return _tokenService.GenerateToken(user.Id, user.Login, user.Role);
        }
    }
}
