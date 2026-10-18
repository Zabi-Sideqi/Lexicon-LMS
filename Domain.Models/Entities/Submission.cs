namespace Domain.Models.Entities;

public class Submission
{
    public int Id { get; set; }

    public int ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    public string StudentId { get; set; } = null!;
    public ApplicationUser Student { get; set; } = null!;

    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }
}