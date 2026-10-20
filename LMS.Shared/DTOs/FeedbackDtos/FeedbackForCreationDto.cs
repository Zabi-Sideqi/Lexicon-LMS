using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.FeedbackDtos
{
    public sealed class FeedbackForCreationDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Text must be between 1 and 2000 characters.")]
        public string Text { get; set; } = string.Empty;

    }
}
