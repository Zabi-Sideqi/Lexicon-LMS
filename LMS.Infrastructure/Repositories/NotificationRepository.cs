
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


    public async Task<IEnumerable<Notification>> GetByRecipientIdAsync(
        string recipientId,
        bool trackChanges = false)
    {
        IQueryable<Notification> query = _context.Notifications
            .Where(notification => notification.RecipientId == recipientId)
            .OrderByDescending(notification => notification.CreatedAt);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<bool> MarkAsReadForRecipientAsync(
        int notificationId,
        string recipientId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(notification =>
                notification.Id == notificationId &&
                notification.RecipientId == recipientId);

        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;

        return true;
    }


    public void Create(Notification notification)
    {
        _context.Notifications.Add(notification);
    }

    public async Task<bool> MarkAsReadAsync(
        int notificationId,
        string studentId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(notification =>
                notification.Id == notificationId &&
                notification.StudentId == studentId);

        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;

        return true;
    }

}
