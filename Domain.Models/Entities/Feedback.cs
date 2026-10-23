using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities
{
    public class Feedback
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; } 
        public Submission Submission { get; set; } = null!;

        public string TeacherId { get; set; } = null!;
        public ApplicationUser Teacher { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    


    }
}
