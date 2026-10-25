namespace EventsRestApi.Application.Exceptions
{
    public abstract class BaseNotFoundException : Exception
    {
        public Guid ResourceId { get; }

        public string ResourceName { get; }

        protected BaseNotFoundException(Guid resourceId, string resourceType, string message)
            : base(message)
        {
            ResourceId = resourceId;
            ResourceName = resourceType;
        }
    }
}
