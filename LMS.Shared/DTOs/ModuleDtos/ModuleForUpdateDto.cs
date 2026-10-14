
using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.ModuleDtos;

public class ModuleForUpdateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be greater than 0.")]
    public int CourseId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(250, MinimumLength = 3)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
