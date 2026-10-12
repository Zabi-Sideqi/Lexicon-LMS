using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.ActivityDtos
{
    internal class ActivityUpdateDto
    {
        [Required]
        [RegularExpression(
            "^(Lecture | Exercise | Assignment)$",
            ErrorMessage = "Activity type must be Lecture, Exercise, or Assignment.")]
        public string Type { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }

    }
}

//Den används när en befintlig aktivitet ska ändras.
//ModuleId ingår inte här eftersom vi i den här designen uppdaterar aktiviteten inom dess befintliga modul.