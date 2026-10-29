using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LMS.Presentation.DTOs;

public class SubmissionUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
