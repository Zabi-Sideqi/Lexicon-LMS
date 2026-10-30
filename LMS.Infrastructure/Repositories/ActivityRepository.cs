
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Extensions;
using LMS.Shared.DTOs.CommonDtos;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync(
        bool trackChanges = false)
    {
        var query = _context.Activities.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<Activity?> GetByIdAsync(
        int id,
        bool trackChanges = false)
    {
        var query = _context.Activities.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(
            activity => activity.Id == id);
    }

    public async Task<IEnumerable<Activity>> GetByModuleIdAsync(
        int moduleId,
        bool trackChanges = false)
    {
        var query = _context.Activities
            .Where(activity => activity.ModuleId == moduleId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByCourseIdAsync(
    int courseId,
    bool trackChanges = false)
    {
        var query = _context.Activities
            .Where(activity => activity.Module.CourseId == courseId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(activity => activity.StartTime)
            .ToListAsync();
    }

    public void Create(Activity activity)
    {
        _context.Activities.Add(activity);
    }

    public void Update(Activity activity)
    {
        _context.Activities.Update(activity);
    }

    public void Delete(Activity activity)
    {
        _context.Activities.Remove(activity);
    }

    public async Task<IEnumerable<Activity>> GetAssignmentsByCourseIdAsync(
    int courseId,
    bool trackChanges = false)
    {
        var query = _context.Activities
            .Where(activity =>
                activity.Type == ActivityType.Assignment &&
                activity.Module.CourseId == courseId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(activity => activity.EndTime)
            .ToListAsync();
    }

    public async Task<PagedList<Activity>> GetPagedAsync(
    PageParameters pageParameters,
    int? courseId = null)
    {
        var query = _context.Activities
            .Include(a => a.Module)
            .AsQueryable();

        // Studenter: begränsa till egen kurs INNAN paging.
        if (courseId is int cid)
        {
            query = query.Where(a => a.Module.CourseId == cid);
        }

        if (!string.IsNullOrWhiteSpace(pageParameters.SearchTerm))
        {
            var search = pageParameters.SearchTerm;
            query = query.Where(a => a.Name.Contains(search));
        }

        query = pageParameters.SortOrder switch
        {
            "start" => query.OrderBy(a => a.StartTime),
            "start_desc" => query.OrderByDescending(a => a.StartTime),
            "name" => query.OrderBy(a => a.Name),
            _ => query.OrderBy(a => a.StartTime)
        };

        return await query.ToPagedListAsync(pageParameters);
    }
}
