namespace EventsRestApi.Domain.Models.Entities
{
    public class Event
    {
        private DateTime _startAt;
        private DateTime _endAt;
        private int _totalSeats;
        private int _availableSeats;

        public Guid Id { get; private set; }

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

        public int TotalSeats
        {
            get => _totalSeats;
        }

        public int AvailableSeats
        {
            get => _availableSeats;
            private set
            {
                ValidateAvailableSeats(value, _totalSeats);
                _availableSeats = value;
            }
        }

        public List<Booking> Bookings { get; private set; } = new ();

        private Event() 
        { 
            Title = null!; 
        }

        public Event(
            Guid id,
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats)
            : this()
        {
            Id = id;

            ValidateTotalSeats(totalSeats);
            SetDates(startAt, endAt);

            Title = title;
            Description = description;

            _totalSeats = totalSeats;
            _availableSeats = totalSeats;
        }

        public bool IsStarted(DateTime currentUtcNow)
        {
            return StartAt < currentUtcNow;
        }

        public bool IsFinished(DateTime currentUtcNow)
        {
            return EndAt < currentUtcNow;
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

        public void SetDates(DateTime newStartAt, DateTime newEndAt)
        {
            ValidateDates(newStartAt, newEndAt, nameof(newEndAt));

            _startAt = newStartAt;
            _endAt = newEndAt;
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

        private void ValidateTotalSeats(int totalSeats)
        {
            if (totalSeats <= 0)
            {
                throw new ArgumentException(
                    $"Событие с Id: {Id}: Количество мест на мероприятии должно быть положительным.",
                    nameof(TotalSeats));
            }
        }

        private void ValidateAvailableSeats(int availableSeats, int totalSeats)
        {
            if (availableSeats < 0)
            {
                throw new ArgumentException(
                    $"Событие с Id: {Id}: Количество доступных мест на мероприятии должно быть неотрицательным.",
                    nameof(AvailableSeats));
            }

            if (availableSeats > totalSeats)
            {
                throw new ArgumentException(
                    $"Событие с Id: {Id}: Количество доступных мест на мероприятии не может быть больше общего количества мест.",
                    nameof(AvailableSeats));
            }
        }
    }
}
