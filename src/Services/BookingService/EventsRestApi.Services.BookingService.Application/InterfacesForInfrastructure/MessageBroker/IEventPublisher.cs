namespace EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.MessageBroker
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string topic, string messageKey, T message, CancellationToken cancellationToken)
            where T : class;
    }
}
