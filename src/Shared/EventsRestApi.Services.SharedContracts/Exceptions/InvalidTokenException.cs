namespace EventsRestApi.Services.SharedContracts.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public string ClaimName { get; }
        public InvalidTokenException(string claimName) :
            base($"Claim {claimName} не найден в токене.")
        {
            ClaimName = claimName;
        }
    }
}
