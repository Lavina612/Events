using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Application.Services.Interfaces;
using EventsRestApi.Application.Settings;
using Microsoft.Extensions.Options;

namespace EventsRestApi.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly UserSettings _userSettings;

        public UserService(IUserRepository userRepository, IOptions<UserSettings> userSettings)
        {
            _userRepository = userRepository;
            _userSettings = userSettings.Value;
        }

        public async Task<bool> CanMakeBooking(Guid userId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetBookingsCount(userId, cancellationToken) < _userSettings.ActiveBookingLimit;
        }
    }
}
