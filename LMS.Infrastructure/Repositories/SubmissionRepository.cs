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


    public async Task<Submission?> GetByIdAsync(
        int submissionId,
        bool trackChanges = false)
    {
        IQueryable<Submission> query = _context.Submissions
            .Where(submission => submission.Id == submissionId)
            .Include(submission => submission.Activity)
                .ThenInclude(activity => activity.Module);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }




    public async Task<IEnumerable<Submission>> GetByCourseIdAsync(
        int courseId, bool trackChanges = false)
    {
        IQueryable<Submission> query = _context.Submissions
            .Where(submission => submission.Activity.Module.CourseId == courseId)
            .Include(submission => submission.Activity)
            .Include(submission => submission.Student)
            .Include(submission => submission.Feedbacks);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(submission => submission.SubmittedAt)
            .ToListAsync();
    }


    public void Create(Submission submission)
    {
        _context.Submissions.Add(submission);
    }
}