
using Domain.Models.Entities;

namespace Service.Contracts;

public interface INotificationService
{
    
    Task<IEnumerable<Notification>> GetByStudentIdAsync(
        string studentId);

    Task<bool> MarkAsReadAsync(
        int notificationId,
        string studentId);

    // Nya generella metoder för både elever och lärare.
    Task<IEnumerable<Notification>> GetByRecipientIdAsync(
        string recipientId);

    Task<bool> MarkAsReadForRecipientAsync(
        int notificationId,
        string recipientId);
}
