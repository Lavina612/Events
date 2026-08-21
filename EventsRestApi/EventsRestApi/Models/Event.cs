namespace EventsRestApi.Models
{
    public class Event
    {
        private DateTime _startAt;
        private DateTime _endAt;

        public Guid Id { get; init; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartAt
        {
            get => _startAt;
            set
            {
                ValidateDates(value, _endAt, nameof(StartAt));
                _startAt = value;
            }
        }

        public DateTime EndAt
        {
            get => _endAt;
            set
            {
                ValidateDates(_startAt, value, nameof(EndAt));
                _endAt = value;
            }
        }

        public int TotalSeats { get; init; }

        public int AvailableSeats { get; private set; }

        public Event(
            Guid id,
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats)
        {
            Id = id;

            if (totalSeats <= 0)
            {
                throw new ArgumentException(
                    $"Событие с Id: {Id}: Количество мест на мероприятии должно быть положительным.",
                    nameof(TotalSeats));
            }

            ValidateDates(startAt, endAt, nameof(endAt));

            Title = title;
            Description = description;
            _startAt = startAt;
            _endAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
        }

        public bool IsStillActual(DateTime currentUtcNow)
        {
            return EndAt > currentUtcNow;
        }

        public bool TryReserveSeats(int count = 1)
        {
            if (AvailableSeats < count)
            {
                return false;
            }

            AvailableSeats -= count;
            return true;
        }

        public void ReleaseSeats(int count = 1)
        {
            if (AvailableSeats + count > TotalSeats)
            {
                throw new InvalidOperationException(
                    $"Невозможно освободить {count} мест, т.к. иначе будет превышено общее число мест. " +
                    $"Сейчас общее число мест: {TotalSeats}, из них свободных мест: {AvailableSeats}.");
            }

            AvailableSeats += count;
        }

        private void ValidateDates(DateTime startAt, DateTime endAt, string paramName)
        {
            if (endAt <= startAt)
            {
                throw new ArgumentException(
                    $"Событие с Id: {Id}: Дата окончания мероприятия должна быть позже даты начала мероприятия.",
                    paramName);
            }
        }
    }
}
