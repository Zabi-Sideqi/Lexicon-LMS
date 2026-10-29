using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LMS.Infrastructure.Extensions;
using LMS.Shared.DTOs.CommonDtos;

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

    public async Task<PagedList<Submission>> GetPagedAsync(
    PageParameters pageParameters,
    bool? hasFeedback = null)
    {
        var query = _context.Submissions
            .Include(s => s.Feedbacks)
            .Include(s => s.Student)
            .Include(s => s.Activity)
            .AsQueryable();

        if (hasFeedback.HasValue)
        {
            query = hasFeedback.Value
                ? query.Where(s => s.Feedbacks.Any())
                : query.Where(s => !s.Feedbacks.Any());
        }

        if (!string.IsNullOrWhiteSpace(pageParameters.SearchTerm))
        {
            var search = pageParameters.SearchTerm;
            query = query.Where(s =>
                s.Student.UserName!.Contains(search) ||
                s.Activity.Name.Contains(search) ||
                s.FileName.Contains(search));
        }

        query = pageParameters.SortOrder switch
        {
            "submitted" => query.OrderBy(s => s.SubmittedAt),
            "submitted_desc" => query.OrderByDescending(s => s.SubmittedAt),
            "student" => query.OrderBy(s => s.Student.UserName),
            _ => query.OrderByDescending(s => s.SubmittedAt)
        };

        return await query.ToPagedListAsync(pageParameters);
    }

    public void Create(Submission submission)
    {
        _context.Submissions.Add(submission);
    }
}