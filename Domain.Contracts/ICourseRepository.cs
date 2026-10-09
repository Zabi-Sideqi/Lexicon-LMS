using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetAllAsync(bool trackChanges = false);
        Task<Course?> GetByIdAsync(int id, bool trackChanges = false);
        void Create(Course course);
    }
}
