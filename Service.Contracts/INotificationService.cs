
using Domain.Models.Entities;

namespace Service.Contracts;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetByStudentIdAsync(
        string studentId);
}
