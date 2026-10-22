namespace EventsRestApi.Domain.Exceptions
{
    public class ExceedingActiveBookingLimitException : Exception
    {
        public Guid UserId { get; }

        public int BookingLimit { get; }

        public ExceedingActiveBookingLimitException(Guid userId, int bookingLimit)
            : base($"Невозможно создать бронирование для пользователя с Id = {userId}, т.к. у него достигнут лимит активных бронирований ({bookingLimit}).")
        {
            UserId = userId;
            BookingLimit = bookingLimit;
        }
    }
}
