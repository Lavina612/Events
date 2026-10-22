namespace EventsRestApi.Domain.Exceptions
{
    public class ForbiddenException : Exception
    {
        public Guid UserId { get; }

        public ForbiddenException(Guid userId)
            : base($"У пользователя с Id = {userId} недостаточно прав для выполнения этой операции.")
        {
            UserId = userId;
        }
    }
}
