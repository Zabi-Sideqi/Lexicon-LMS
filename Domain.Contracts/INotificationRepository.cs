
using Domain.Models.Entities;

namespace Domain.Contracts;

public interface INotificationRepository
{
  
    Task<IEnumerable<Notification>> GetByStudentIdAsync(
        string studentId,
        bool trackChanges = false);

    Task<bool> MarkAsReadAsync(
        int notificationId,
        string studentId);

    // Nya generella metoder för elever och lärare.
    Task<IEnumerable<Notification>> GetByRecipientIdAsync(
        string recipientId,
        bool trackChanges = false);

    Task<bool> MarkAsReadForRecipientAsync(
        int notificationId,
        string recipientId);

    void Create(Notification notification);
}
