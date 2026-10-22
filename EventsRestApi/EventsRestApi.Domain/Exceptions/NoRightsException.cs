namespace EventsRestApi.Domain.Exceptions
{
    public class NoRightsException : Exception
    {
        public Guid UserId { get; }

        public NoRightsException(Guid userId)
            : base($"У пользователя с Id = {userId} недостаточно прав для выполнения этой операции.")
        {
            UserId = userId;
        }
    }
}
