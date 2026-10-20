using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpireTime { get; set; }


    public int? CourseId { get; set; }
    public Course? Course { get; set; }

    // Kurser som läraren undervisar i
    public ICollection<Course> TeachingCourses { get; set; } = [];
}
