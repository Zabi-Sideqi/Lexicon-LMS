
namespace Domain.Models.Entities;

public class Activity
{
    public int Id { get; set; }

    public int ModuleId { get; set; }

    public ActivityType Type { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public Module Module { get; set; } = null!;
}
