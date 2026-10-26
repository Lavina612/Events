namespace EventsRestApi.Services.SharedContracts.Exceptions
{
    public class NotFoundResourceException<T> : BaseNotFoundException
    {
        public NotFoundResourceException(Guid resourceId)
            : base(resourceId, typeof(T).Name, $"Ресурс {typeof(T).Name} с Id = {resourceId} не найден.") { }
    }
}
