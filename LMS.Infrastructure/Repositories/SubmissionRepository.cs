using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly ApplicationDbContext _context;

    public SubmissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Submission>> GetByStudentIdAsync(
        string studentId,
        bool trackChanges = false)
    {
        var query = _context.Submissions
            .Where(submission => submission.StudentId == studentId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public void Create(Submission submission)
    {
        _context.Submissions.Add(submission);
    }
}