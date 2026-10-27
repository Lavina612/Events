namespace EventsRestApi.Services.SharedContracts.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base($"У вас недостаточно прав для выполнения этой операции.") { }
    }
}
