
using Domain.Models.Entities;

namespace Domain.Contracts;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByStudentIdAsync(
        string studentId,
        bool trackChanges = false);

    void Create(Notification notification);
}
