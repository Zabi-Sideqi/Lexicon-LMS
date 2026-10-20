
using Domain.Contracts;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetByStudentIdAsync(
        string studentId,
        bool trackChanges = false)
    {
        IQueryable<Notification> query = _context.Notifications
            .Where(notification => notification.StudentId == studentId)
            .OrderByDescending(notification => notification.CreatedAt);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public void Create(Notification notification)
    {
        _context.Notifications.Add(notification);
    }
}
