using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Document>> GetByCourseIdAsync(
        int courseId, bool trackChanges = false)
    {
        var query = _context.Documents
            .Include(d => d.Course)
            .Include(d => d.Module).ThenInclude(m => m!.Course)
            .Include(d => d.Activity).ThenInclude(a => a!.Module).ThenInclude(m => m!.Course)
            .Where(d =>
            d.CourseId == courseId ||
            (d.ModuleId != null && d.Module!.CourseId == courseId) ||
            (d.ActivityId != null && d.Activity!.Module!.CourseId == courseId));

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByModuleIdAsync(
        int moduleId, bool trackChanges = false)
    {
        var query = _context.Documents
            .Where(d => d.ModuleId == moduleId);

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(int id, bool trackChanges = false)
    {
        var query = _context.Documents
            .Include(d => d.Course)
            .Include(d => d.Module).ThenInclude(m => m.Course)
            .Include(d => d.Activity).ThenInclude(a => a.Module).ThenInclude(m => m.Course)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(d => d.Id == id);
    }

    public void Create(Document document) => _context.Documents.Add(document);

    public void Delete(Document document) => _context.Documents.Remove(document);
}