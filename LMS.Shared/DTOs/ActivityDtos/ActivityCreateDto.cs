using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.ActivityDtos;

public class ActivityCreateDto
{
    [Required]
    public int ModuleId { get; set; }

    [Required]
    [RegularExpression(
        "^(Lecture|ELearning|Exercise|Assignment|Other)$",
        ErrorMessage = "Activity type must be Lecture, ELearning, Exercise, Assignment or Other.")]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}
