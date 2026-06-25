namespace EventsRestApi.Exceptions
{
    public class AppValidationException : Exception
    {
        public Dictionary<string, string[]> Errors { get; } = new();

        public AppValidationException(string message) : base(message) { }

        public AppValidationException(string message, Dictionary<string, string[]> errors)
        : base(message)
        {
            Errors = errors;
        }
    }
}
