using Domain.Models.Entities;
using LMS.Shared.DTOs.CommonDtos;

namespace Domain.Contracts;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllAsync(bool trackChanges = false);
    Task<Course?> GetByIdAsync(int id, bool trackChanges = false);
    Task<Course?> GetCourseWithStudentsAsync(int id, bool trackChanges = false);
    Task<IEnumerable<Course>> GetByTeacherIdAsync(
        string TeacherId,
        bool trackChanges = false);
    Task<PagedList<Course>> GetPagedAsync(PageParameters pageParameters);
    void Create(Course course);
}