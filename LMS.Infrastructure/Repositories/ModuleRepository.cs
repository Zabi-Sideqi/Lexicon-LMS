
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly ApplicationDbContext _context;

    public ModuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Module>> GetAllAsync(
        bool trackChanges = false)
    {
        var query = _context.Modules.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<Module?> GetByIdAsync(
        int id,
        bool trackChanges = false)
    {
        var query = _context.Modules.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(
            module => module.Id == id);
    }

    public async Task<IEnumerable<Module>> GetByCourseIdAsync(
        int courseId,
        bool trackChanges = false)
    {
        var query = _context.Modules
            .Where(module => module.CourseId == courseId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public void Create(Module module)
    {
        _context.Modules.Add(module);
    }

    public void Update(Module module)
    {
        _context.Modules.Update(module);
    }

    public void Delete(Module module)
    {
        _context.Modules.Remove(module);
    }
}
