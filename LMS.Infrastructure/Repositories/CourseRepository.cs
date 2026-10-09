using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class CourseRepository(ApplicationDbContext context) : ICourseRepository
{
    public async Task<IEnumerable<Course>> GetAllAsync(bool trackChanges = false)
    {
        var query = context.Courses.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id, bool trackChanges = false)
    {
        var query = context.Courses.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Course?> GetCourseWithStudentsAsync(int id, bool trackChanges = false)
    {
        var query = context.Courses
            .Include(c => c.Students)
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public void Create(Course course) => context.Courses.Add(course);
}