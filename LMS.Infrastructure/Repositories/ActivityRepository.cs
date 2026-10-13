
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
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
}
