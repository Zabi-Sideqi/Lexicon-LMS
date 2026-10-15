using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.UserDtos
{
    public class UserCreateDto : IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "^(Teacher|Student)$",
            ErrorMessage = "Role must be either 'Teacher' or 'Student'.")]
        public string Role { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (Role == "Student" &&
                (!CourseId.HasValue || CourseId.Value <= 0))
            {
                yield return new ValidationResult(
                    "Välj en kurs för studenten.",
                    new[] { nameof(CourseId) });
            }
        }
    }
}
