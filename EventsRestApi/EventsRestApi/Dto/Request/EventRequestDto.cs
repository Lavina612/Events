using System.ComponentModel.DataAnnotations;

namespace EventsRestApi.Dto.Request
{
    public class EventRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "Название мероприятия обязательно для заполнения.")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Дата начала мероприятия обязательна для заполнения.")]
        public DateTime StartAt { get; set; }

        [Required(ErrorMessage = "Дата окончания мероприятия обязательна для заполнения.")]
        public DateTime EndAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult(
                    "Дата окончания мероприятия должна быть позже даты начала мероприятия.",
                    [nameof(EndAt)]
                );
            }
        }

        public EventRequestDto() { }

        public EventRequestDto(
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt)
        {
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}