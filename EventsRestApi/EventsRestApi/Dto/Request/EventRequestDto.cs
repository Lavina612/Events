using System.ComponentModel.DataAnnotations;

namespace EventsRestApi.Dto.Request
{
    public record EventRequestDto(
        [Required(ErrorMessage = "Название мероприятия обязательно для заполнения.")] string Title,
        string? Description,
        [Required(ErrorMessage = "Дата начала мероприятия обязательна для заполнения.")] DateTime StartAt,
        [Required(ErrorMessage = "Дата окончания мероприятия обязательна для заполнения.")] DateTime EndAt)
        : IValidatableObject
    {
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
    }
}