
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetAllAsync(
        bool trackChanges = false)
    {
        var query = _context.Courses.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(
        int id,
        bool trackChanges = false)
    {
        var query = _context.Courses.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(course => course.Id == id);
    }

    public void Create(Course course)
    {
        _context.Courses.Add(course);
    }
}
