namespace EventsRestApi.Application.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base($"У вас недостаточно прав для выполнения этой операции.") { }
    }
}
