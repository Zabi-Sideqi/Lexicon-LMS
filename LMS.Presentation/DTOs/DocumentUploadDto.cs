using Microsoft.AspNetCore.Http;

namespace LMS.Presentation.DTOs;

public class DocumentUploadDto
{
    public IFormFile File { get; set; } = null!;
    public int? CourseId { get; set; }
    public int? ModuleId { get; set; }
    public int? ActivityId { get; set; }
}