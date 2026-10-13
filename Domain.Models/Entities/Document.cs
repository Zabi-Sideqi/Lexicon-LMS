namespace Domain.Models.Entities;

public class Document
{
    public int Id { get; set; }

    public string FileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }
    public string UploadedByUserId { get; set; } = null!;

    public int? CourseId { get; set; }
    public int? ModuleId { get; set; }

    public ApplicationUser UploadedBy { get; set; } = null!;
    public Course? Course { get; set; }
    public Module? Module { get; set; }
}