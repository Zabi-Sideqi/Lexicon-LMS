
namespace Domain.Models.Entities;

public class Notification
{
    public int Id { get; set; }

    public string StudentId { get; set; } = null!;
    public ApplicationUser Student { get; set; } = null!;

    public int FeedbackId { get; set; }
    public Feedback Feedback { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }
}
