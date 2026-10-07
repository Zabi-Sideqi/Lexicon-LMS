using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<ApplicationUser> Students { get; set; } = [];
    }
}
