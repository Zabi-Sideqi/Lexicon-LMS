
namespace Domain.Models.Entities;

public class Notification
{
    public int Id { get; set; }

    // Befintlig koppling till äldre studentnotiser.
    // Behålls tills vi har migrerat och uppdaterat användningen.
    public string? StudentId { get; set; }
    public ApplicationUser? Student { get; set; }

    // Ny generell mottagare: elev eller lärare.
    public string? RecipientId { get; set; }
    public ApplicationUser? Recipient { get; set; }

    public int? FeedbackId { get; set; }
    public Feedback? Feedback { get; set; }

    public int? DocumentId { get; set; }
    public Document? Document { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }
}
