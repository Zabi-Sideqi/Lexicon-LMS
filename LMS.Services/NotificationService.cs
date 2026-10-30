
using Domain.Contracts;
using Domain.Models.Entities;
using Service.Contracts;

namespace LMS.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Notification>> GetByStudentIdAsync(
        string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));
        }

        return await _uow.NotificationRepository
            .GetByStudentIdAsync(studentId);
    }

    public async Task<bool> MarkAsReadAsync(
        int notificationId,
        string studentId)
    {
        if (notificationId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(notificationId),
                "Notification ID must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(studentId))
        {
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));
        }

        var notificationUpdated = await _uow.NotificationRepository
            .MarkAsReadAsync(notificationId, studentId);

        if (!notificationUpdated)
        {
            return false;
        }

        await _uow.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Notification>> GetByRecipientIdAsync(
        string recipientId)
    {
        if (string.IsNullOrWhiteSpace(recipientId))
        {
            throw new ArgumentException(
                "Recipient ID cannot be empty.",
                nameof(recipientId));
        }

        return await _uow.NotificationRepository
            .GetByRecipientIdAsync(recipientId);
    }

    public async Task<bool> MarkAsReadForRecipientAsync(
        int notificationId,
        string recipientId)
    {
        if (notificationId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(notificationId),
                "Notification ID must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            throw new ArgumentException(
                "Recipient ID cannot be empty.",
                nameof(recipientId));
        }

        var notificationUpdated = await _uow.NotificationRepository
            .MarkAsReadForRecipientAsync(notificationId, recipientId);

        if (!notificationUpdated)
        {
            return false;
        }

        await _uow.SaveChangesAsync();

        return true;
    }

}
