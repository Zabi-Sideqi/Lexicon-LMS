namespace Domain.Models.Entities;

public class Submission
{
    public int Id { get; set; }

    public int ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    public string StudentId { get; set; } = null!;
    public ApplicationUser Student { get; set; } = null!;

    public string FileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }

    public DateTime SubmittedAt { get; set; }

    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
