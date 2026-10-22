namespace EventsRestApi.Application.Exceptions
{
    public class NotFoundResourceException<T> : BaseNotFoundException
    {
        public NotFoundResourceException(Guid id)
            : base(id, typeof(T).Name, $"Ресурс {typeof(T).Name} с Id = {id} не найден.") { }
    }
}
