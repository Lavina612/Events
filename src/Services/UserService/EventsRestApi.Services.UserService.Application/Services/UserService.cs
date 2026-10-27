using EventsRestApi.Services.SharedContracts.Exceptions;
using EventsRestApi.Services.SharedContracts.Models;
using EventsRestApi.Services.UserService.Application.Dto.Request;
using EventsRestApi.Services.UserService.Application.Exceptions;
using EventsRestApi.Services.UserService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.UserService.Application.InterfacesForInfrastructure.Services;
using EventsRestApi.Services.UserService.Application.Services.Interfaces;
using EventsRestApi.Services.UserService.Domain.Models.Entities;

namespace EventsRestApi.Services.UserService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IPasswordService _passwordService;

        private readonly ITokenService _tokenService;

        public UserService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _tokenService = tokenService;
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
