namespace EventsRestApi.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> CanMakeBooking(Guid userId, CancellationToken cancellationToken);
    }
}
