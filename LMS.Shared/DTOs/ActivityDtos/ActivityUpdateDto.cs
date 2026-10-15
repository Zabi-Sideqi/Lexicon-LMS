using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.ActivityDtos;

public class ActivityUpdateDto
{
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

// Used when updating an existing activity.
// ModuleId is omitted because the activity remains in its existing module.
