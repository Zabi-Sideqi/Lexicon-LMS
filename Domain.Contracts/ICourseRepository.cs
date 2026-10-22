using Domain.Models.Entities;

namespace Domain.Contracts;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllAsync(bool trackChanges = false);
    Task<Course?> GetByIdAsync(int id, bool trackChanges = false);
    Task<Course?> GetCourseWithStudentsAsync(int id, bool trackChanges = false);
    Task<IEnumerable<Course>> GetByTeacherIdAsync(
        string TeacherId,
        bool trackChanges = false);
    void Create(Course course);
}