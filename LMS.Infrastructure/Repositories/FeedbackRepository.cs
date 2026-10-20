
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ApplicationDbContext _context;

    public FeedbackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Feedback>> GetBySubmissionIdAsync(
        int submissionId,
        bool trackChanges = false)
    {
        var query = _context.Feedbacks
            .Where(feedback => feedback.SubmissionId == submissionId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(feedback => feedback.CreatedAt)
            .ToListAsync();
    }

    public void Create(Feedback feedback)
    {
        _context.Feedbacks.Add(feedback);
    }
}
