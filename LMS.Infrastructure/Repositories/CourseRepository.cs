using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.CommonDtos;
using Microsoft.EntityFrameworkCore;
using LMS.Infrastructure.Extensions;

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
        IQueryable<Course> query = _context.Courses
            .Include(course => course.Teachers);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(
            course => course.Id == id);
    }


    public async Task<Course?> GetCourseWithStudentsAsync(
        int id,
        bool trackChanges = false)
    {
        var query = _context.Courses
            .Include(c => c.Students)
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(
            course => course.Id == id);
    }
    public async Task<IEnumerable<Course>> GetByTeacherIdAsync(
    string teacherId,
    bool trackChanges = false)
    {
        IQueryable<Course> query = _context.Courses
            .Where(course => course.Teachers.Any(
                teacher => teacher.Id == teacherId));

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }
    public void Create(Course course)
    {
        _context.Courses.Add(course);
    }

    public async Task<PagedList<Course>> GetPagedAsync(PageParameters pageParameters)
    {
        var query = _context.Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pageParameters.SearchTerm))
        {
            query = query.Where(c => c.Name.Contains(pageParameters.SearchTerm));
        }

        query = pageParameters.SortOrder switch
        {
            "name" => query.OrderBy(c => c.Name),
            "name_desc" => query.OrderByDescending(c => c.Name),
            "startDate" => query.OrderBy(c => c.StartDate),
            "endDate" => query.OrderBy(c => c.EndDate),
            _ => query.OrderBy(c => c.Id)
        };

        return await query.ToPagedListAsync(pageParameters);
    }
}