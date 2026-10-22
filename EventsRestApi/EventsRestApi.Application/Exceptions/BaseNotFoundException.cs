namespace EventsRestApi.Application.Exceptions
{
    public abstract class BaseNotFoundException : Exception
    {
        public Guid Id { get; }

        public string ResourceName { get; }

        protected BaseNotFoundException(Guid id, string resourceType, string message)
            : base(message)
        {
            Id = id;
            ResourceName = resourceType;
        }
    }
}
